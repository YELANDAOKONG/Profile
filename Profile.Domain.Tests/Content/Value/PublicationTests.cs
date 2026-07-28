using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Value;

public sealed class PublicationTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static TheoryData<Publication> NonDraftStates =>
        new()
        {
            InState(PublicationStatus.Scheduled),
            InState(PublicationStatus.PendingReview),
            InState(PublicationStatus.Published)
        };

    public static TheoryData<Publication> NonScheduledStates =>
        new()
        {
            InState(PublicationStatus.Draft),
            InState(PublicationStatus.PendingReview),
            InState(PublicationStatus.Published)
        };

    public static TheoryData<Publication> NonPendingReviewStates =>
        new()
        {
            InState(PublicationStatus.Draft),
            InState(PublicationStatus.Scheduled),
            InState(PublicationStatus.Published)
        };

    public static TheoryData<Publication> NonPublishedStates =>
        new()
        {
            InState(PublicationStatus.Draft),
            InState(PublicationStatus.Scheduled),
            InState(PublicationStatus.PendingReview)
        };

    [Fact]
    public void CreateDraft_ReturnsDraftWithoutTimestamps()
    {
        var publication = Publication.CreateDraft();

        Assert.Equal(PublicationStatus.Draft, publication.Status);
        Assert.Null(publication.ScheduledAt);
        Assert.Null(publication.FirstPublishedAt);
        Assert.Null(publication.LastPublishedAt);
    }

    [Fact]
    public void Schedule_FromDraft_ReturnsScheduledWithTime()
    {
        var scheduledAt = _baseTime.AddDays(1);

        var result = Publication.CreateDraft().Schedule(scheduledAt, _baseTime);

        Assert.Equal(PublicationStatus.Scheduled, result.Status);
        Assert.Equal(scheduledAt, result.ScheduledAt);
    }

    [Fact]
    public void Schedule_AfterUnpublish_PreservesPublishHistory()
    {
        var publishedAt = _baseTime;
        var unpublished = PublishAt(publishedAt).Unpublish();
        var scheduledAt = publishedAt.AddDays(1);

        var result = unpublished.Schedule(scheduledAt, publishedAt);

        Assert.Equal(PublicationStatus.Scheduled, result.Status);
        Assert.Equal(publishedAt, result.FirstPublishedAt);
        Assert.Equal(publishedAt, result.LastPublishedAt);
    }

    [Fact]
    public void Schedule_WithTimeNotInFuture_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => Publication.CreateDraft().Schedule(_baseTime, _baseTime));

        Assert.Equal("scheduledAt", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(NonDraftStates))]
    public void Schedule_FromNonDraftState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(
            () => publication.Schedule(_baseTime.AddDays(2), _baseTime));
    }

    [Fact]
    public void Unschedule_FromScheduled_ReturnsDraftAndClearsScheduledTime()
    {
        var scheduled = Publication.CreateDraft()
            .Schedule(_baseTime.AddDays(1), _baseTime);

        var result = scheduled.Unschedule();

        Assert.Equal(PublicationStatus.Draft, result.Status);
        Assert.Null(result.ScheduledAt);
    }

    [Theory]
    [MemberData(nameof(NonScheduledStates))]
    public void Unschedule_FromNonScheduledState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(() => publication.Unschedule());
    }

    [Fact]
    public void SubmitForReview_FromDraft_ReturnsPendingReview()
    {
        var result = Publication.CreateDraft().SubmitForReview();

        Assert.Equal(PublicationStatus.PendingReview, result.Status);
    }

    [Theory]
    [MemberData(nameof(NonDraftStates))]
    public void SubmitForReview_FromNonDraftState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(
            () => publication.SubmitForReview());
    }

    [Fact]
    public void Approve_FromPendingReview_PublishesAndRecordsFirstAndLastTimes()
    {
        var pending = Publication.CreateDraft().SubmitForReview();

        var result = pending.Approve(_baseTime);

        Assert.Equal(PublicationStatus.Published, result.Status);
        Assert.Equal(_baseTime, result.FirstPublishedAt);
        Assert.Equal(_baseTime, result.LastPublishedAt);
        Assert.Null(result.ScheduledAt);
    }

    [Fact]
    public void Approve_WhenRepublishing_KeepsFirstTimeAndUpdatesLastTime()
    {
        var firstPublishedAt = _baseTime;
        var republishedAt = _baseTime.AddDays(1);
        var returned = PublishAt(firstPublishedAt)
            .Unpublish()
            .SubmitForReview();

        var result = returned.Approve(republishedAt);

        Assert.Equal(firstPublishedAt, result.FirstPublishedAt);
        Assert.Equal(republishedAt, result.LastPublishedAt);
    }

    [Fact]
    public void Approve_WithTimeEarlierThanPreviousPublish_ThrowsArgumentOutOfRangeException()
    {
        var publishedAt = _baseTime.AddDays(1);
        var returned = PublishAt(publishedAt)
            .Unpublish()
            .SubmitForReview();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => returned.Approve(_baseTime));

        Assert.Equal("publishedAt", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(NonPendingReviewStates))]
    public void Approve_FromNonPendingReviewState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(
            () => publication.Approve(_baseTime.AddDays(2)));
    }

    [Fact]
    public void PublishScheduled_FromScheduled_PublishesAtScheduledTime()
    {
        var scheduledAt = _baseTime.AddDays(1);
        var scheduled = Publication.CreateDraft().Schedule(scheduledAt, _baseTime);

        var result = scheduled.PublishScheduled(scheduledAt);

        Assert.Equal(PublicationStatus.Published, result.Status);
        Assert.Equal(scheduledAt, result.FirstPublishedAt);
        Assert.Equal(scheduledAt, result.LastPublishedAt);
        Assert.Null(result.ScheduledAt);
    }

    [Fact]
    public void PublishScheduled_BeforeScheduledTime_ThrowsArgumentOutOfRangeException()
    {
        var scheduledAt = _baseTime.AddDays(1);
        var scheduled = Publication.CreateDraft().Schedule(scheduledAt, _baseTime);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => scheduled.PublishScheduled(_baseTime));

        Assert.Equal("publishedAt", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(NonScheduledStates))]
    public void PublishScheduled_FromNonScheduledState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(
            () => publication.PublishScheduled(_baseTime.AddDays(2)));
    }

    [Fact]
    public void ReturnToDraft_FromPendingReview_ReturnsDraft()
    {
        var pending = Publication.CreateDraft().SubmitForReview();

        var result = pending.ReturnToDraft();

        Assert.Equal(PublicationStatus.Draft, result.Status);
    }

    [Theory]
    [MemberData(nameof(NonPendingReviewStates))]
    public void ReturnToDraft_FromNonPendingReviewState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(() => publication.ReturnToDraft());
    }

    [Fact]
    public void Unpublish_FromPublished_ReturnsDraftAndRetainsPublishTimes()
    {
        var publishedAt = _baseTime;

        var result = PublishAt(publishedAt).Unpublish();

        Assert.Equal(PublicationStatus.Draft, result.Status);
        Assert.Equal(publishedAt, result.FirstPublishedAt);
        Assert.Equal(publishedAt, result.LastPublishedAt);
    }

    [Theory]
    [MemberData(nameof(NonPublishedStates))]
    public void Unpublish_FromNonPublishedState_ThrowsInvalidOperationException(
        Publication publication)
    {
        Assert.Throws<InvalidOperationException>(() => publication.Unpublish());
    }

    [Fact]
    public void Reconstitute_WithValidState_PreservesValues()
    {
        var firstPublishedAt = _baseTime;
        var lastPublishedAt = _baseTime.AddDays(1);

        var publication = Publication.Reconstitute(
            PublicationStatus.Draft,
            null,
            firstPublishedAt,
            lastPublishedAt);

        Assert.Equal(PublicationStatus.Draft, publication.Status);
        Assert.Equal(firstPublishedAt, publication.FirstPublishedAt);
        Assert.Equal(lastPublishedAt, publication.LastPublishedAt);
    }

    [Fact]
    public void Reconstitute_WithUndefinedStatus_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => Publication.Reconstitute((PublicationStatus)999, null, null, null));

        Assert.Equal("status", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_ScheduledWithoutScheduledTime_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => Publication.Reconstitute(
                PublicationStatus.Scheduled,
                null,
                null,
                null));

        Assert.Equal("scheduledAt", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_NonScheduledWithScheduledTime_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => Publication.Reconstitute(
                PublicationStatus.Draft,
                _baseTime,
                null,
                null));

        Assert.Equal("scheduledAt", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_PublishedWithoutPublishTimes_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                null,
                null));

        Assert.Equal("firstPublishedAt", exception.ParamName);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Reconstitute_WithOnlyOnePublishTime_ThrowsArgumentException(
        bool hasFirst,
        bool hasLast)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => Publication.Reconstitute(
                PublicationStatus.Draft,
                null,
                hasFirst ? _baseTime : null,
                hasLast ? _baseTime : null));

        Assert.Equal("lastPublishedAt", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithLastTimeBeforeFirstTime_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                _baseTime.AddDays(1),
                _baseTime));

        Assert.Equal("lastPublishedAt", exception.ParamName);
    }

    private static Publication InState(PublicationStatus status) =>
        status switch
        {
            PublicationStatus.Draft => Publication.CreateDraft(),
            PublicationStatus.Scheduled => Publication.CreateDraft()
                .Schedule(_baseTime.AddDays(1), _baseTime),
            PublicationStatus.PendingReview => Publication.CreateDraft()
                .SubmitForReview(),
            PublicationStatus.Published => PublishAt(_baseTime),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Publication status is not supported.")
        };

    private static Publication PublishAt(DateTimeOffset publishedAt) =>
        Publication.CreateDraft().SubmitForReview().Approve(publishedAt);
}

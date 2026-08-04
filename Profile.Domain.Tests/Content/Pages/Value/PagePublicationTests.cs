using Profile.Domain.Content.Pages.Value;

namespace Profile.Domain.Tests.Content.Pages.Value;

public sealed class PagePublicationTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateDraft_HasNoPublishHistory()
    {
        var publication = PagePublication.CreateDraft();

        Assert.Equal(PagePublicationStatus.Draft, publication.Status);
        Assert.Null(publication.FirstPublishedAt);
        Assert.Null(publication.LastPublishedAt);
    }

    [Fact]
    public void Publish_FromDraft_RecordsFirstAndLastPublishTimes()
    {
        var publication = PagePublication.CreateDraft();

        var published = publication.Publish(_baseTime);

        Assert.Equal(PagePublicationStatus.Published, published.Status);
        Assert.Equal(_baseTime, published.FirstPublishedAt);
        Assert.Equal(_baseTime, published.LastPublishedAt);
        Assert.Equal(PagePublicationStatus.Draft, publication.Status);
    }

    [Fact]
    public void Unpublish_FromPublished_RetainsPublishHistory()
    {
        var publication = PagePublication
            .CreateDraft()
            .Publish(_baseTime);

        var draft = publication.Unpublish();

        Assert.Equal(PagePublicationStatus.Draft, draft.Status);
        Assert.Equal(_baseTime, draft.FirstPublishedAt);
        Assert.Equal(_baseTime, draft.LastPublishedAt);
    }

    [Fact]
    public void Publish_AfterUnpublish_RetainsFirstAndUpdatesLastTime()
    {
        var republishedAt = _baseTime.AddDays(1);
        var publication = PagePublication
            .CreateDraft()
            .Publish(_baseTime)
            .Unpublish();

        var republished = publication.Publish(republishedAt);

        Assert.Equal(_baseTime, republished.FirstPublishedAt);
        Assert.Equal(republishedAt, republished.LastPublishedAt);
    }

    [Fact]
    public void Publish_BeforePreviousPublishTime_ThrowsArgumentOutOfRangeException()
    {
        var publication = PagePublication
            .CreateDraft()
            .Publish(_baseTime)
            .Unpublish();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => publication.Publish(_baseTime.AddTicks(-1)));
    }

    [Fact]
    public void Publish_FromPublished_ThrowsInvalidOperationException()
    {
        var publication = PagePublication
            .CreateDraft()
            .Publish(_baseTime);

        Assert.Throws<InvalidOperationException>(
            () => publication.Publish(_baseTime.AddMinutes(1)));
    }

    [Fact]
    public void Unpublish_FromDraft_ThrowsInvalidOperationException()
    {
        var publication = PagePublication.CreateDraft();

        Assert.Throws<InvalidOperationException>(
            publication.Unpublish);
    }

    [Fact]
    public void Reconstitute_WithPublishedState_PreservesHistory()
    {
        var lastPublishedAt = _baseTime.AddDays(1);

        var publication = PagePublication.Reconstitute(
            PagePublicationStatus.Published,
            _baseTime,
            lastPublishedAt);

        Assert.Equal(PagePublicationStatus.Published, publication.Status);
        Assert.Equal(_baseTime, publication.FirstPublishedAt);
        Assert.Equal(lastPublishedAt, publication.LastPublishedAt);
    }

    [Fact]
    public void Reconstitute_WithUnsupportedStatus_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PagePublication.Reconstitute(
                (PagePublicationStatus)int.MaxValue,
                null,
                null));
    }

    [Fact]
    public void Reconstitute_PublishedWithoutHistory_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => PagePublication.Reconstitute(
                PagePublicationStatus.Published,
                null,
                null));
    }

    [Fact]
    public void Reconstitute_WithOnlyFirstPublishTime_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => PagePublication.Reconstitute(
                PagePublicationStatus.Draft,
                _baseTime,
                null));
    }

    [Fact]
    public void Reconstitute_WithLastBeforeFirst_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PagePublication.Reconstitute(
                PagePublicationStatus.Draft,
                _baseTime,
                _baseTime.AddTicks(-1)));
    }
}

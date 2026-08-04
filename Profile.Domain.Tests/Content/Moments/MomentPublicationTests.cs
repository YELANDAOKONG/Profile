using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Moments;

public sealed class MomentPublicationTests
{
    public static TheoryData<string> PublicationOperations =>
    [
        nameof(Moment.Schedule),
        nameof(Moment.Unschedule),
        nameof(Moment.SubmitForReview),
        nameof(Moment.Approve),
        nameof(Moment.PublishScheduled),
        nameof(Moment.ReturnToDraft),
        nameof(Moment.UnpublishToDraft)
    ];

    [Fact]
    public void Schedule_FromDraft_SetsScheduledState()
    {
        var moment = MomentTestFactory.CreateMoment();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);
        var scheduledAt = changedAt.AddDays(1);

        moment.Schedule(scheduledAt, changedAt);

        Assert.Equal(PublicationStatus.Scheduled, moment.Publication.Status);
        Assert.Equal(scheduledAt, moment.Publication.ScheduledAt);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void Unschedule_FromScheduled_ReturnsToDraft()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                MomentTestFactory.BaseTime.AddDays(1),
                null,
                null));
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.Unschedule(changedAt);

        Assert.Equal(PublicationStatus.Draft, moment.Publication.Status);
        Assert.Null(moment.Publication.ScheduledAt);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void SubmitForReview_FromDraft_SetsPendingReview()
    {
        var moment = MomentTestFactory.CreateMoment();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.SubmitForReview(changedAt);

        Assert.Equal(PublicationStatus.PendingReview, moment.Publication.Status);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void Approve_FromPendingReview_PublishesAndRecordsTime()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.PendingReview,
                null,
                null,
                null));
        var publishedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.Approve(publishedAt);

        Assert.Equal(PublicationStatus.Published, moment.Publication.Status);
        Assert.Equal(publishedAt, moment.Publication.FirstPublishedAt);
        Assert.Equal(publishedAt, moment.Publication.LastPublishedAt);
        Assert.Equal(publishedAt, moment.UpdatedAt);
    }

    [Fact]
    public void PublishScheduled_AtScheduledTime_Publishes()
    {
        var scheduledAt = MomentTestFactory.BaseTime.AddDays(1);
        var moment = MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                scheduledAt,
                null,
                null));

        moment.PublishScheduled(scheduledAt);

        Assert.Equal(PublicationStatus.Published, moment.Publication.Status);
        Assert.Null(moment.Publication.ScheduledAt);
        Assert.Equal(scheduledAt, moment.Publication.FirstPublishedAt);
        Assert.Equal(scheduledAt, moment.Publication.LastPublishedAt);
        Assert.Equal(scheduledAt, moment.UpdatedAt);
    }

    [Fact]
    public void ReturnToDraft_FromPendingReview_ReturnsToDraft()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.PendingReview,
                null,
                null,
                null));
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.ReturnToDraft(changedAt);

        Assert.Equal(PublicationStatus.Draft, moment.Publication.Status);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void UnpublishToDraft_FromPublished_RetainsPublishHistory()
    {
        var moment = CreatePublishedMoment();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.UnpublishToDraft(changedAt);

        Assert.Equal(PublicationStatus.Draft, moment.Publication.Status);
        Assert.Equal(
            MomentTestFactory.BaseTime,
            moment.Publication.FirstPublishedAt);
        Assert.Equal(
            MomentTestFactory.BaseTime,
            moment.Publication.LastPublishedAt);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void Approve_WhenRepublishing_RetainsFirstAndUpdatesLastTime()
    {
        var moment = CreatePublishedMoment();

        moment.UnpublishToDraft(MomentTestFactory.BaseTime.AddMinutes(1));
        moment.SubmitForReview(MomentTestFactory.BaseTime.AddMinutes(2));
        moment.Approve(MomentTestFactory.BaseTime.AddMinutes(3));

        Assert.Equal(
            MomentTestFactory.BaseTime,
            moment.Publication.FirstPublishedAt);
        Assert.Equal(
            MomentTestFactory.BaseTime.AddMinutes(3),
            moment.Publication.LastPublishedAt);
    }

    [Fact]
    public void UnpublishToDraft_FromDraft_ThrowsAndPreservesState()
    {
        var moment = MomentTestFactory.CreateMoment();
        var publication = moment.Publication;

        Assert.Throws<InvalidOperationException>(
            () => moment.UnpublishToDraft(
                MomentTestFactory.BaseTime.AddMinutes(1)));

        Assert.Same(publication, moment.Publication);
        Assert.Equal(MomentTestFactory.BaseTime, moment.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(PublicationOperations))]
    public void PublicationOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var moment = CreateMomentForOperation(operation);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeOperation(
                moment,
                operation,
                moment.UpdatedAt.AddTicks(-1)));
    }

    [Theory]
    [MemberData(nameof(PublicationOperations))]
    public void PublicationOperation_WhenDeleted_ThrowsInvalidOperationException(
        string operation)
    {
        var moment = CreateMomentForOperation(operation, deleted: true);

        Assert.Throws<InvalidOperationException>(
            () => InvokeOperation(moment, operation, moment.UpdatedAt));
    }

    private static Moment CreatePublishedMoment() =>
        MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                MomentTestFactory.BaseTime,
                MomentTestFactory.BaseTime));

    private static Moment CreateMomentForOperation(
        string operation,
        bool deleted = false)
    {
        var publication = operation switch
        {
            nameof(Moment.Schedule) or nameof(Moment.SubmitForReview) =>
                Publication.CreateDraft(),
            nameof(Moment.Unschedule) or nameof(Moment.PublishScheduled) =>
                Publication.Reconstitute(
                    PublicationStatus.Scheduled,
                    MomentTestFactory.BaseTime.AddDays(1),
                    null,
                    null),
            nameof(Moment.Approve) or nameof(Moment.ReturnToDraft) =>
                Publication.Reconstitute(
                    PublicationStatus.PendingReview,
                    null,
                    null,
                    null),
            nameof(Moment.UnpublishToDraft) =>
                Publication.Reconstitute(
                    PublicationStatus.Published,
                    null,
                    MomentTestFactory.BaseTime,
                    MomentTestFactory.BaseTime),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "Moment publication operation is not supported.")
        };

        if (!deleted)
        {
            return MomentTestFactory.ReconstituteMoment(publication);
        }

        var deletedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        return MomentTestFactory.ReconstituteMoment(
            publication,
            ContentDeletion.Create(deletedAt),
            updatedAt: deletedAt);
    }

    private static void InvokeOperation(
        Moment moment,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Moment.Schedule):
                moment.Schedule(changedAt.AddDays(1), changedAt);
                break;
            case nameof(Moment.Unschedule):
                moment.Unschedule(changedAt);
                break;
            case nameof(Moment.SubmitForReview):
                moment.SubmitForReview(changedAt);
                break;
            case nameof(Moment.Approve):
                moment.Approve(changedAt);
                break;
            case nameof(Moment.PublishScheduled):
                moment.PublishScheduled(changedAt);
                break;
            case nameof(Moment.ReturnToDraft):
                moment.ReturnToDraft(changedAt);
                break;
            case nameof(Moment.UnpublishToDraft):
                moment.UnpublishToDraft(changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Moment publication operation is not supported.");
        }
    }
}

using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Moments;

public sealed class MomentDeletionTests
{
    [Fact]
    public void Delete_FromDraft_EntersRecoveryPeriod()
    {
        var moment = MomentTestFactory.CreateMoment();
        var deletedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.Delete(deletedAt);

        Assert.Equal(deletedAt, moment.Deletion?.DeletedAt);
        Assert.Equal(
            deletedAt.AddDays(ContentDeletion.RecoveryPeriodDays),
            moment.Deletion?.PurgeAt);
        Assert.Equal(deletedAt, moment.UpdatedAt);
    }

    [Fact]
    public void Delete_FromPublished_PreservesPublicationState()
    {
        var publication = CreatePublishedPublication();
        var moment = MomentTestFactory.ReconstituteMoment(publication);

        moment.Delete(MomentTestFactory.BaseTime.AddMinutes(1));

        Assert.Same(publication, moment.Publication);
        Assert.Equal(PublicationStatus.Published, moment.Publication.Status);
    }

    [Fact]
    public void Restore_BeforePurge_ClearsDeletionAndPreservesPublication()
    {
        var publication = CreatePublishedPublication();
        var moment = CreateDeletedMoment(publication);
        var restoredAt = moment.Deletion!.PurgeAt.AddTicks(-1);

        moment.Restore(restoredAt);

        Assert.Null(moment.Deletion);
        Assert.Same(publication, moment.Publication);
        Assert.Equal(restoredAt, moment.UpdatedAt);
    }

    [Fact]
    public void Restore_AtPurgeTime_ThrowsAndPreservesDeletion()
    {
        var moment = CreateDeletedMoment(Publication.CreateDraft());
        var deletion = moment.Deletion!;

        Assert.Throws<InvalidOperationException>(
            () => moment.Restore(deletion.PurgeAt));

        Assert.Same(deletion, moment.Deletion);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ThrowsInvalidOperationException()
    {
        var moment = CreateDeletedMoment(Publication.CreateDraft());

        Assert.Throws<InvalidOperationException>(
            () => moment.Delete(moment.UpdatedAt.AddMinutes(1)));
    }

    [Fact]
    public void Restore_WhenActive_ThrowsInvalidOperationException()
    {
        var moment = MomentTestFactory.CreateMoment();

        Assert.Throws<InvalidOperationException>(
            () => moment.Restore(MomentTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void UnpublishAndDiscard_FromPublished_MovesDraftToRecoveryPeriod()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            CreatePublishedPublication());
        var deletedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.UnpublishAndDiscard(deletedAt);

        Assert.Equal(PublicationStatus.Draft, moment.Publication.Status);
        Assert.Equal(
            MomentTestFactory.BaseTime,
            moment.Publication.FirstPublishedAt);
        Assert.Equal(deletedAt, moment.Deletion?.DeletedAt);
        Assert.Equal(deletedAt, moment.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_FromDraft_ThrowsAndDoesNotDelete()
    {
        var moment = MomentTestFactory.CreateMoment();

        Assert.Throws<InvalidOperationException>(
            () => moment.UnpublishAndDiscard(
                MomentTestFactory.BaseTime.AddMinutes(1)));

        Assert.Null(moment.Deletion);
        Assert.Equal(MomentTestFactory.BaseTime, moment.UpdatedAt);
    }

    [Theory]
    [InlineData(nameof(Moment.Delete))]
    [InlineData(nameof(Moment.Restore))]
    [InlineData(nameof(Moment.UnpublishAndDiscard))]
    public void DeletionOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var moment = operation is nameof(Moment.Restore)
            ? CreateDeletedMoment(Publication.CreateDraft())
            : CreateMomentForOperation(operation);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeOperation(
                moment,
                operation,
                moment.UpdatedAt.AddTicks(-1)));
    }

    [Fact]
    public void RecoveryQueries_ReflectDeletionBoundary()
    {
        var activeMoment = MomentTestFactory.CreateMoment();
        var deletedMoment = CreateDeletedMoment(Publication.CreateDraft());
        var deletion = deletedMoment.Deletion!;

        Assert.False(activeMoment.CanRestoreAt(MomentTestFactory.BaseTime));
        Assert.False(activeMoment.IsReadyForPurgeAt(MomentTestFactory.BaseTime));
        Assert.True(deletedMoment.CanRestoreAt(deletion.DeletedAt));
        Assert.True(deletedMoment.CanRestoreAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.False(deletedMoment.CanRestoreAt(deletion.PurgeAt));
        Assert.False(
            deletedMoment.IsReadyForPurgeAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.True(deletedMoment.IsReadyForPurgeAt(deletion.PurgeAt));
    }

    private static Publication CreatePublishedPublication() =>
        Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            MomentTestFactory.BaseTime,
            MomentTestFactory.BaseTime);

    private static Moment CreateDeletedMoment(Publication publication)
    {
        var deletedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        return MomentTestFactory.ReconstituteMoment(
            publication,
            ContentDeletion.Create(deletedAt),
            updatedAt: deletedAt);
    }

    private static Moment CreateMomentForOperation(string operation) =>
        operation switch
        {
            nameof(Moment.Delete) => MomentTestFactory.CreateMoment(),
            nameof(Moment.UnpublishAndDiscard) =>
                MomentTestFactory.ReconstituteMoment(
                    CreatePublishedPublication()),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "Moment deletion operation is not supported.")
        };

    private static void InvokeOperation(
        Moment moment,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Moment.Delete):
                moment.Delete(changedAt);
                break;
            case nameof(Moment.Restore):
                moment.Restore(changedAt);
                break;
            case nameof(Moment.UnpublishAndDiscard):
                moment.UnpublishAndDiscard(changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Moment deletion operation is not supported.");
        }
    }
}

using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Pages;
using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Pages;

public sealed class PageLifecycleTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Publish_FromDraft_RecordsHistoryAndUpdatesTime()
    {
        var page = CreatePage();
        var publishedAt = _baseTime.AddMinutes(1);

        page.Publish(publishedAt);

        Assert.Equal(PagePublicationStatus.Published, page.Publication.Status);
        Assert.Equal(publishedAt, page.Publication.FirstPublishedAt);
        Assert.Equal(publishedAt, page.Publication.LastPublishedAt);
        Assert.Equal(publishedAt, page.UpdatedAt);
    }

    [Fact]
    public void Publish_WithEmptyBody_AllowsPage()
    {
        var page = CreatePage();

        page.Publish(_baseTime.AddMinutes(1));

        Assert.Equal(PagePublicationStatus.Published, page.Publication.Status);
    }

    [Fact]
    public void UnpublishToDraft_FromPublished_RetainsHistory()
    {
        var page = CreatePage();
        var publishedAt = _baseTime.AddMinutes(1);
        var changedAt = _baseTime.AddMinutes(2);
        page.Publish(publishedAt);

        page.UnpublishToDraft(changedAt);

        Assert.Equal(PagePublicationStatus.Draft, page.Publication.Status);
        Assert.Equal(publishedAt, page.Publication.FirstPublishedAt);
        Assert.Equal(publishedAt, page.Publication.LastPublishedAt);
        Assert.Equal(changedAt, page.UpdatedAt);
    }

    [Fact]
    public void Publish_AfterUnpublish_RetainsFirstAndUpdatesLastTime()
    {
        var page = CreatePage();
        var firstPublishedAt = _baseTime.AddMinutes(1);
        var lastPublishedAt = _baseTime.AddMinutes(3);
        page.Publish(firstPublishedAt);
        page.UnpublishToDraft(_baseTime.AddMinutes(2));

        page.Publish(lastPublishedAt);

        Assert.Equal(firstPublishedAt, page.Publication.FirstPublishedAt);
        Assert.Equal(lastPublishedAt, page.Publication.LastPublishedAt);
    }

    [Fact]
    public void Publish_FromPublished_DoesNotChangePage()
    {
        var page = CreatePage();
        var publishedAt = _baseTime.AddMinutes(1);
        page.Publish(publishedAt);
        var publication = page.Publication;

        Assert.Throws<InvalidOperationException>(
            () => page.Publish(_baseTime.AddMinutes(2)));

        Assert.Same(publication, page.Publication);
        Assert.Equal(publishedAt, page.UpdatedAt);
    }

    [Fact]
    public void PublicationOperation_WhenPageIsDeleted_ThrowsInvalidOperationException()
    {
        var page = CreatePage();
        page.Delete(_baseTime.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            () => page.Publish(_baseTime.AddMinutes(2)));
    }

    [Fact]
    public void PublicationOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException()
    {
        var page = CreatePage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => page.Publish(_baseTime.AddTicks(-1)));

        Assert.Equal(PagePublicationStatus.Draft, page.Publication.Status);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void Delete_FromPublished_PreservesPublicationAndRoute()
    {
        var page = CreatePage();
        var routeIdentifier = page.RouteIdentifier;
        page.Publish(_baseTime.AddMinutes(1));
        var deletedAt = _baseTime.AddMinutes(2);

        page.Delete(deletedAt);

        Assert.Equal(PagePublicationStatus.Published, page.Publication.Status);
        Assert.Equal(routeIdentifier, page.RouteIdentifier);
        Assert.Equal(deletedAt, page.Deletion?.DeletedAt);
        Assert.Equal(
            deletedAt.AddDays(ContentDeletion.RecoveryPeriodDays),
            page.Deletion?.PurgeAt);
        Assert.Equal(deletedAt, page.UpdatedAt);
    }

    [Fact]
    public void Restore_BeforePurge_ClearsDeletionAndPreservesPublication()
    {
        var page = CreatePage();
        page.Publish(_baseTime.AddMinutes(1));
        page.Delete(_baseTime.AddMinutes(2));
        var restoredAt = _baseTime.AddMinutes(3);

        page.Restore(restoredAt);

        Assert.Null(page.Deletion);
        Assert.Equal(PagePublicationStatus.Published, page.Publication.Status);
        Assert.Equal(restoredAt, page.UpdatedAt);
    }

    [Fact]
    public void Restore_AtPurgeTime_ThrowsAndKeepsDeletion()
    {
        var page = CreatePage();
        page.Delete(_baseTime.AddMinutes(1));
        var deletion = page.Deletion!;

        Assert.Throws<InvalidOperationException>(
            () => page.Restore(deletion.PurgeAt));

        Assert.Same(deletion, page.Deletion);
        Assert.Equal(deletion.DeletedAt, page.UpdatedAt);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ThrowsAndKeepsDeletion()
    {
        var page = CreatePage();
        page.Delete(_baseTime.AddMinutes(1));
        var deletion = page.Deletion;

        Assert.Throws<InvalidOperationException>(
            () => page.Delete(_baseTime.AddMinutes(2)));

        Assert.Same(deletion, page.Deletion);
        Assert.Equal(_baseTime.AddMinutes(1), page.UpdatedAt);
    }

    [Fact]
    public void Restore_WhenNotDeleted_ThrowsAndDoesNotChangePage()
    {
        var page = CreatePage();

        Assert.Throws<InvalidOperationException>(
            () => page.Restore(_baseTime.AddMinutes(1)));

        Assert.Null(page.Deletion);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_FromPublished_MovesDraftToRecoveryPeriod()
    {
        var page = CreatePage();
        page.Publish(_baseTime.AddMinutes(1));
        var deletedAt = _baseTime.AddMinutes(2);

        page.UnpublishAndDiscard(deletedAt);

        Assert.Equal(PagePublicationStatus.Draft, page.Publication.Status);
        Assert.Equal(_baseTime.AddMinutes(1), page.Publication.FirstPublishedAt);
        Assert.Equal(_baseTime.AddMinutes(1), page.Publication.LastPublishedAt);
        Assert.Equal(deletedAt, page.Deletion?.DeletedAt);
        Assert.Equal(deletedAt, page.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_FromDraft_DoesNotDeletePage()
    {
        var page = CreatePage();

        Assert.Throws<InvalidOperationException>(
            () => page.UnpublishAndDiscard(_baseTime.AddMinutes(1)));

        Assert.Null(page.Deletion);
        Assert.Equal(PagePublicationStatus.Draft, page.Publication.Status);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void RecoveryQueries_ReflectDeletionBoundary()
    {
        var page = CreatePage();
        page.Delete(_baseTime.AddMinutes(1));
        var deletion = page.Deletion!;

        Assert.True(page.CanRestoreAt(deletion.DeletedAt));
        Assert.True(page.CanRestoreAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.False(page.CanRestoreAt(deletion.PurgeAt));
        Assert.False(page.IsReadyForPurgeAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.True(page.IsReadyForPurgeAt(deletion.PurgeAt));
    }

    [Fact]
    public void DeletionOperation_WithEarlierTime_DoesNotChangePage()
    {
        var page = CreatePage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => page.Delete(_baseTime.AddTicks(-1)));

        Assert.Null(page.Deletion);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    private static Page CreatePage() =>
        Page.Create(
            PageIdentity.New(),
            UserIdentity.New(),
            new PageRouteIdentifier("About"),
            "Page title",
            new ContentBlockCollection([]),
            ContentVisibility.Public,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            null,
            null,
            null,
            _baseTime);
}

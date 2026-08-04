using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Posts;

public sealed class PostDeletionTests
{
    [Fact]
    public void Delete_FromDraft_EntersRecoveryPeriod()
    {
        var post = PostTestFactory.CreatePost();
        var deletedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.Delete(deletedAt);

        Assert.Equal(deletedAt, post.Deletion?.DeletedAt);
        Assert.Equal(
            deletedAt.AddDays(ContentDeletion.RecoveryPeriodDays),
            post.Deletion?.PurgeAt);
        Assert.Equal(deletedAt, post.UpdatedAt);
    }

    [Fact]
    public void Delete_FromPublished_PreservesPublicationState()
    {
        var publication = CreatePublishedPublication();
        var post = PostTestFactory.ReconstitutePost(publication);

        post.Delete(PostTestFactory.BaseTime.AddMinutes(1));

        Assert.Same(publication, post.Publication);
        Assert.Equal(PublicationStatus.Published, post.Publication.Status);
    }

    [Fact]
    public void Restore_BeforePurge_ClearsDeletionAndPreservesPublication()
    {
        var publication = CreatePublishedPublication();
        var post = CreateDeletedPost(publication);
        var restoredAt = post.Deletion!.PurgeAt.AddTicks(-1);

        post.Restore(restoredAt);

        Assert.Null(post.Deletion);
        Assert.Same(publication, post.Publication);
        Assert.Equal(restoredAt, post.UpdatedAt);
    }

    [Fact]
    public void Restore_AtPurgeTime_ThrowsAndPreservesDeletion()
    {
        var post = CreateDeletedPost(Publication.CreateDraft());
        var deletion = post.Deletion!;

        Assert.Throws<InvalidOperationException>(
            () => post.Restore(deletion.PurgeAt));

        Assert.Same(deletion, post.Deletion);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ThrowsInvalidOperationException()
    {
        var post = CreateDeletedPost(Publication.CreateDraft());

        Assert.Throws<InvalidOperationException>(
            () => post.Delete(post.UpdatedAt.AddMinutes(1)));
    }

    [Fact]
    public void Restore_WhenActive_ThrowsInvalidOperationException()
    {
        var post = PostTestFactory.CreatePost();

        Assert.Throws<InvalidOperationException>(
            () => post.Restore(PostTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void UnpublishAndDiscard_FromPublished_MovesDraftToRecoveryPeriod()
    {
        var post = PostTestFactory.ReconstitutePost(
            CreatePublishedPublication());
        var deletedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.UnpublishAndDiscard(deletedAt);

        Assert.Equal(PublicationStatus.Draft, post.Publication.Status);
        Assert.Equal(
            PostTestFactory.BaseTime,
            post.Publication.FirstPublishedAt);
        Assert.Equal(deletedAt, post.Deletion?.DeletedAt);
        Assert.Equal(deletedAt, post.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_FromDraft_ThrowsAndDoesNotDelete()
    {
        var post = PostTestFactory.CreatePost();

        Assert.Throws<InvalidOperationException>(
            () => post.UnpublishAndDiscard(
                PostTestFactory.BaseTime.AddMinutes(1)));

        Assert.Null(post.Deletion);
        Assert.Equal(PostTestFactory.BaseTime, post.UpdatedAt);
    }

    [Theory]
    [InlineData(nameof(Post.Delete))]
    [InlineData(nameof(Post.Restore))]
    [InlineData(nameof(Post.UnpublishAndDiscard))]
    public void DeletionOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var post = operation is nameof(Post.Restore)
            ? CreateDeletedPost(Publication.CreateDraft())
            : CreatePostForOperation(operation);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeOperation(
                post,
                operation,
                post.UpdatedAt.AddTicks(-1)));
    }

    [Fact]
    public void RecoveryQueries_ReflectDeletionBoundary()
    {
        var activePost = PostTestFactory.CreatePost();
        var deletedPost = CreateDeletedPost(Publication.CreateDraft());
        var deletion = deletedPost.Deletion!;

        Assert.False(activePost.CanRestoreAt(PostTestFactory.BaseTime));
        Assert.False(activePost.IsReadyForPurgeAt(PostTestFactory.BaseTime));
        Assert.True(deletedPost.CanRestoreAt(deletion.DeletedAt));
        Assert.True(deletedPost.CanRestoreAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.False(deletedPost.CanRestoreAt(deletion.PurgeAt));
        Assert.False(
            deletedPost.IsReadyForPurgeAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.True(deletedPost.IsReadyForPurgeAt(deletion.PurgeAt));
    }

    private static Publication CreatePublishedPublication() =>
        Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            PostTestFactory.BaseTime,
            PostTestFactory.BaseTime);

    private static Post CreateDeletedPost(Publication publication)
    {
        var deletedAt = PostTestFactory.BaseTime.AddMinutes(1);

        return PostTestFactory.ReconstitutePost(
            publication,
            ContentDeletion.Create(deletedAt),
            updatedAt: deletedAt);
    }

    private static Post CreatePostForOperation(string operation) =>
        operation switch
        {
            nameof(Post.Delete) => PostTestFactory.CreatePost(),
            nameof(Post.UnpublishAndDiscard) =>
                PostTestFactory.ReconstitutePost(
                    CreatePublishedPublication()),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "Post deletion operation is not supported.")
        };

    private static void InvokeOperation(
        Post post,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Post.Delete):
                post.Delete(changedAt);
                break;
            case nameof(Post.Restore):
                post.Restore(changedAt);
                break;
            case nameof(Post.UnpublishAndDiscard):
                post.UnpublishAndDiscard(changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Post deletion operation is not supported.");
        }
    }
}

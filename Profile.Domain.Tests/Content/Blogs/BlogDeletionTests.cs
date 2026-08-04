using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Blogs;

public sealed class BlogDeletionTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Delete_FromDraft_EntersRecoveryPeriodAndUpdatesTime()
    {
        var blog = CreateBlog(Publication.CreateDraft());
        var publication = blog.Publication;
        var deletedAt = _baseTime.AddMinutes(1);

        blog.Delete(deletedAt);

        Assert.Same(publication, blog.Publication);
        Assert.Equal(deletedAt, blog.Deletion?.DeletedAt);
        Assert.Equal(
            deletedAt.AddDays(ContentDeletion.RecoveryPeriodDays),
            blog.Deletion?.PurgeAt);
        Assert.Equal(deletedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Delete_FromPublished_PreservesPublicationStateAndHistory()
    {
        var publication = CreatePublishedPublication();
        var blog = CreateBlog(publication);

        blog.Delete(_baseTime.AddMinutes(1));

        Assert.Same(publication, blog.Publication);
        Assert.Equal(PublicationStatus.Published, blog.Publication.Status);
        Assert.Equal(_baseTime, blog.Publication.FirstPublishedAt);
        Assert.Equal(_baseTime, blog.Publication.LastPublishedAt);
    }

    [Fact]
    public void Restore_BeforePurge_ClearsDeletionAndPreservesPublication()
    {
        var publication = CreatePublishedPublication();
        var blog = CreateDeletedBlog(publication);
        var restoredAt = blog.Deletion!.PurgeAt.AddTicks(-1);

        blog.Restore(restoredAt);

        Assert.Null(blog.Deletion);
        Assert.Same(publication, blog.Publication);
        Assert.Equal(restoredAt, blog.UpdatedAt);
    }

    [Fact]
    public void Restore_AtPurgeTime_ThrowsAndDoesNotChangeBlog()
    {
        var blog = CreateDeletedBlog(Publication.CreateDraft());
        var deletion = blog.Deletion!;

        Assert.Throws<InvalidOperationException>(
            () => blog.Restore(deletion.PurgeAt));

        Assert.Same(deletion, blog.Deletion);
        Assert.Equal(deletion.DeletedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ThrowsAndDoesNotChangeBlog()
    {
        var blog = CreateDeletedBlog(Publication.CreateDraft());
        var deletion = blog.Deletion!;

        Assert.Throws<InvalidOperationException>(
            () => blog.Delete(deletion.DeletedAt.AddMinutes(1)));

        Assert.Same(deletion, blog.Deletion);
        Assert.Equal(deletion.DeletedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Restore_WhenNotDeleted_ThrowsAndDoesNotChangeBlog()
    {
        var blog = CreateBlog(Publication.CreateDraft());

        Assert.Throws<InvalidOperationException>(
            () => blog.Restore(_baseTime.AddMinutes(1)));

        Assert.Null(blog.Deletion);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Theory]
    [InlineData(nameof(Blog.Delete), "deletedAt")]
    [InlineData(nameof(Blog.Restore), "restoredAt")]
    [InlineData(nameof(Blog.UnpublishAndDiscard), "deletedAt")]
    public void DeletionOperation_WithEarlierTime_ThrowsAndDoesNotChangeBlog(
        string operation,
        string expectedParameter)
    {
        var blog = operation is nameof(Blog.Restore)
            ? CreateDeletedBlog(Publication.CreateDraft())
            : CreateBlogForOperation(operation);
        var publication = blog.Publication;
        var deletion = blog.Deletion;
        var updatedAt = blog.UpdatedAt;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeOperation(blog, operation, updatedAt.AddTicks(-1)));

        Assert.Equal(expectedParameter, exception.ParamName);
        Assert.Same(publication, blog.Publication);
        Assert.Same(deletion, blog.Deletion);
        Assert.Equal(updatedAt, blog.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_FromPublished_MovesDraftToRecoveryPeriod()
    {
        var blog = CreateBlog(CreatePublishedPublication());
        var deletedAt = _baseTime.AddMinutes(1);

        blog.UnpublishAndDiscard(deletedAt);

        Assert.Equal(PublicationStatus.Draft, blog.Publication.Status);
        Assert.Equal(_baseTime, blog.Publication.FirstPublishedAt);
        Assert.Equal(_baseTime, blog.Publication.LastPublishedAt);
        Assert.Equal(deletedAt, blog.Deletion?.DeletedAt);
        Assert.Equal(deletedAt, blog.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_FromDraft_ThrowsAndDoesNotDeleteBlog()
    {
        var blog = CreateBlog(Publication.CreateDraft());
        var publication = blog.Publication;

        Assert.Throws<InvalidOperationException>(
            () => blog.UnpublishAndDiscard(_baseTime.AddMinutes(1)));

        Assert.Same(publication, blog.Publication);
        Assert.Null(blog.Deletion);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void UnpublishAndDiscard_WhenDeleted_ThrowsAndDoesNotChangeBlog()
    {
        var blog = CreateDeletedBlog(CreatePublishedPublication());
        var publication = blog.Publication;
        var deletion = blog.Deletion!;

        Assert.Throws<InvalidOperationException>(
            () => blog.UnpublishAndDiscard(deletion.DeletedAt.AddMinutes(1)));

        Assert.Same(publication, blog.Publication);
        Assert.Same(deletion, blog.Deletion);
        Assert.Equal(deletion.DeletedAt, blog.UpdatedAt);
    }

    [Fact]
    public void RecoveryQueries_ReflectDeletionBoundary()
    {
        var activeBlog = CreateBlog(Publication.CreateDraft());
        var deletedBlog = CreateDeletedBlog(Publication.CreateDraft());
        var deletion = deletedBlog.Deletion!;

        Assert.False(activeBlog.CanRestoreAt(_baseTime));
        Assert.False(activeBlog.IsReadyForPurgeAt(_baseTime));
        Assert.True(deletedBlog.CanRestoreAt(deletion.DeletedAt));
        Assert.True(deletedBlog.CanRestoreAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.False(deletedBlog.CanRestoreAt(deletion.PurgeAt));
        Assert.False(
            deletedBlog.IsReadyForPurgeAt(deletion.PurgeAt.AddTicks(-1)));
        Assert.True(deletedBlog.IsReadyForPurgeAt(deletion.PurgeAt));
    }

    private static Publication CreatePublishedPublication() =>
        Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            _baseTime,
            _baseTime);

    private static Blog CreateBlogForOperation(string operation) =>
        operation switch
        {
            nameof(Blog.Delete) => CreateBlog(Publication.CreateDraft()),
            nameof(Blog.UnpublishAndDiscard) =>
                CreateBlog(CreatePublishedPublication()),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "Deletion operation is not supported.")
        };

    private static Blog CreateDeletedBlog(Publication publication)
    {
        var deletedAt = _baseTime.AddMinutes(1);

        return CreateBlog(
            publication,
            ContentDeletion.Create(deletedAt),
            deletedAt);
    }

    private static Blog CreateBlog(
        Publication publication,
        ContentDeletion? deletion = null,
        DateTimeOffset? updatedAt = null) =>
        Blog.Reconstitute(
            BlogIdentity.New(),
            UserIdentity.New(),
            new BlogSlug("000000001"),
            "Blog title",
            new ContentBlockCollection([]),
            null,
            null,
            ContentVisibility.Public,
            null,
            [],
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            pinned: false,
            featured: false,
            null,
            null,
            null,
            [],
            publication,
            deletion,
            _baseTime,
            updatedAt ?? _baseTime);

    private static void InvokeOperation(
        Blog blog,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Blog.Delete):
                blog.Delete(changedAt);
                break;
            case nameof(Blog.Restore):
                blog.Restore(changedAt);
                break;
            case nameof(Blog.UnpublishAndDiscard):
                blog.UnpublishAndDiscard(changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Deletion operation is not supported.");
        }
    }
}

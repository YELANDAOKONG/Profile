using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Blogs;

public sealed class BlogRevisionWorkflowTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AutosaveBody_ReplacesWorkingCopyWithoutCreatingRevision()
    {
        var blog = CreateBlog();
        var blocks = CreateBlocks("Autosaved");
        var changedAt = _baseTime.AddMinutes(1);

        blog.AutosaveBody(blocks, changedAt);

        Assert.Same(blocks, blog.Blocks);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void SaveBody_ReplacesWorkingCopyAndReturnsManualRevision()
    {
        var blog = CreateBlog();
        var blocks = CreateBlocks("Saved");
        var savedAt = _baseTime.AddMinutes(1);

        var revision = blog.SaveBody(blocks, savedAt);

        Assert.Same(blocks, blog.Blocks);
        Assert.Equal(blog.Id, revision.BlogId);
        Assert.Same(blocks, revision.Blocks);
        Assert.Equal(BlogRevisionCause.ManualSave, revision.Cause);
        Assert.Equal(savedAt, revision.CreatedAt);
        Assert.Equal(savedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Save_WithUnchangedBody_StillCreatesDistinctRevisions()
    {
        var blog = CreateBlog();

        var first = blog.Save(_baseTime.AddMinutes(1));
        var second = blog.Save(_baseTime.AddMinutes(2));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Same(blog.Blocks, first.Blocks);
        Assert.Same(blog.Blocks, second.Blocks);
    }

    [Fact]
    public void Approve_ReturnsPublishedBodyRevision()
    {
        var blog = CreateBlog(CreateBlocks("Published"));
        blog.SubmitForReview(_baseTime.AddMinutes(1));
        var publishedAt = _baseTime.AddMinutes(2);

        var revision = blog.Approve(publishedAt);

        Assert.Equal(PublicationStatus.Published, blog.Publication.Status);
        Assert.Equal(BlogRevisionCause.Publish, revision.Cause);
        Assert.Same(blog.Blocks, revision.Blocks);
        Assert.Equal(publishedAt, revision.CreatedAt);
    }

    [Fact]
    public void PublishScheduled_ReturnsPublishedBodyRevision()
    {
        var blog = CreateBlog(CreateBlocks("Scheduled"));
        var scheduledAt = _baseTime.AddHours(1);
        blog.Schedule(scheduledAt, _baseTime.AddMinutes(1));

        var revision = blog.PublishScheduled(scheduledAt);

        Assert.Equal(PublicationStatus.Published, blog.Publication.Status);
        Assert.Equal(BlogRevisionCause.Publish, revision.Cause);
        Assert.Same(blog.Blocks, revision.Blocks);
        Assert.Equal(scheduledAt, revision.CreatedAt);
    }

    [Fact]
    public void SaveBody_WhenPublished_ChangesBodyInPlaceAndKeepsPublishedState()
    {
        var blog = CreateBlog();
        blog.SubmitForReview(_baseTime.AddMinutes(1));
        blog.Approve(_baseTime.AddMinutes(2));
        var changedBlocks = CreateBlocks("Changed while published");

        var revision = blog.SaveBody(
            changedBlocks,
            _baseTime.AddMinutes(3));

        Assert.Equal(PublicationStatus.Published, blog.Publication.Status);
        Assert.Same(changedBlocks, blog.Blocks);
        Assert.Same(changedBlocks, revision.Blocks);
    }

    [Fact]
    public void Rollback_SavesCurrentBodyBeforeLoadingTargetSnapshot()
    {
        var originalBlocks = CreateBlocks("Original");
        var changedBlocks = CreateBlocks("Changed");
        var blog = CreateBlog(originalBlocks);
        var targetRevision = BlogRevision.Create(
            blog.Id,
            originalBlocks,
            BlogRevisionCause.ManualSave,
            _baseTime);
        blog.AutosaveBody(changedBlocks, _baseTime.AddMinutes(1));
        var rolledBackAt = _baseTime.AddMinutes(2);

        var rollbackRevision = blog.Rollback(
            targetRevision,
            rolledBackAt);

        Assert.Same(originalBlocks, blog.Blocks);
        Assert.Same(changedBlocks, rollbackRevision.Blocks);
        Assert.Equal(BlogRevisionCause.Rollback, rollbackRevision.Cause);
        Assert.Equal(rolledBackAt, rollbackRevision.CreatedAt);
        Assert.Equal(rolledBackAt, blog.UpdatedAt);
    }

    [Fact]
    public void Rollback_WithRevisionFromAnotherBlog_DoesNotChangeBody()
    {
        var blocks = CreateBlocks("Current");
        var blog = CreateBlog(blocks);
        var targetRevision = BlogRevision.Create(
            BlogIdentity.New(),
            CreateBlocks("Other"),
            BlogRevisionCause.ManualSave,
            _baseTime);

        Assert.Throws<ArgumentException>(
            () => blog.Rollback(
                targetRevision,
                _baseTime.AddMinutes(1)));

        Assert.Same(blocks, blog.Blocks);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void Rollback_BeforeTargetRevisionTime_DoesNotChangeBody()
    {
        var blocks = CreateBlocks("Current");
        var blog = CreateBlog(blocks);
        var targetRevision = BlogRevision.Create(
            blog.Id,
            CreateBlocks("Future"),
            BlogRevisionCause.ManualSave,
            _baseTime.AddMinutes(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => blog.Rollback(
                targetRevision,
                _baseTime.AddMinutes(1)));

        Assert.Same(blocks, blog.Blocks);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void RevisionOperation_WhenBlogIsDeleted_ThrowsInvalidOperationException()
    {
        var blog = CreateBlog();
        blog.Delete(_baseTime.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            () => blog.Save(_baseTime.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(
            () => blog.SaveBody(
                CreateBlocks("Changed"),
                _baseTime.AddMinutes(2)));
    }

    private static Blog CreateBlog(
        ContentBlockCollection? blocks = null) =>
        Blog.Create(
            BlogIdentity.New(),
            UserIdentity.New(),
            new BlogSlug("000000001"),
            "Blog title",
            blocks ?? new ContentBlockCollection([]),
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
            _baseTime);

    private static ContentBlockCollection CreateBlocks(string source) =>
        new(
        [
            new TextBlock(new ContentBody(source, ContentFormat.Markdown))
        ]);
}

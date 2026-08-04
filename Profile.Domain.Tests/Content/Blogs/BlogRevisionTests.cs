using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Blogs;

public sealed class BlogRevisionTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesImmutableSnapshot()
    {
        var blogId = BlogIdentity.New();
        var blocks = CreateBlocks("Snapshot");

        var revision = BlogRevision.Create(
            blogId,
            blocks,
            BlogRevisionCause.ManualSave,
            _createdAt);

        Assert.NotEqual(Guid.Empty, revision.Id.Value);
        Assert.Equal(blogId, revision.BlogId);
        Assert.Same(blocks, revision.Blocks);
        Assert.Equal(BlogRevisionCause.ManualSave, revision.Cause);
        Assert.Equal(_createdAt, revision.CreatedAt);
    }

    [Fact]
    public void Reconstitute_WithCompleteState_PreservesIdentity()
    {
        var revisionId = BlogRevisionIdentity.New();

        var revision = BlogRevision.Reconstitute(
            revisionId,
            BlogIdentity.New(),
            CreateBlocks("Snapshot"),
            BlogRevisionCause.Rollback,
            _createdAt);

        Assert.Equal(revisionId, revision.Id);
        Assert.Equal(BlogRevisionCause.Rollback, revision.Cause);
    }

    [Fact]
    public void Reconstitute_WithUnsupportedCause_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BlogRevision.Reconstitute(
                BlogRevisionIdentity.New(),
                BlogIdentity.New(),
                CreateBlocks("Snapshot"),
                (BlogRevisionCause)int.MaxValue,
                _createdAt));
    }

    [Fact]
    public void Reconstitute_WithNullBlocks_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => BlogRevision.Reconstitute(
                BlogRevisionIdentity.New(),
                BlogIdentity.New(),
                null!,
                BlogRevisionCause.ManualSave,
                _createdAt));
    }

    private static ContentBlockCollection CreateBlocks(string source) =>
        new(
        [
            new TextBlock(new ContentBody(source, ContentFormat.Markdown))
        ]);
}

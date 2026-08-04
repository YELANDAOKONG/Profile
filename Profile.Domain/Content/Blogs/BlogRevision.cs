using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs.Value;

namespace Profile.Domain.Content.Blogs;

public sealed class BlogRevision
{
    private BlogRevision(
        BlogRevisionIdentity id,
        BlogIdentity blogId,
        ContentBlockCollection blocks,
        BlogRevisionCause cause,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(blogId);
        ArgumentNullException.ThrowIfNull(blocks);

        if (!Enum.IsDefined(cause))
        {
            throw new ArgumentOutOfRangeException(
                nameof(cause),
                cause,
                "Blog revision cause is not supported.");
        }

        Id = id;
        BlogId = blogId;
        Blocks = blocks;
        Cause = cause;
        CreatedAt = createdAt;
    }

    public BlogRevisionIdentity Id { get; }

    public BlogIdentity BlogId { get; }

    public ContentBlockCollection Blocks { get; }

    public BlogRevisionCause Cause { get; }

    public DateTimeOffset CreatedAt { get; }

    public static BlogRevision Create(
        BlogIdentity blogId,
        ContentBlockCollection blocks,
        BlogRevisionCause cause,
        DateTimeOffset createdAt) =>
        new(
            BlogRevisionIdentity.New(),
            blogId,
            blocks,
            cause,
            createdAt);

    public static BlogRevision Reconstitute(
        BlogRevisionIdentity id,
        BlogIdentity blogId,
        ContentBlockCollection blocks,
        BlogRevisionCause cause,
        DateTimeOffset createdAt) =>
        new(
            id,
            blogId,
            blocks,
            cause,
            createdAt);
}

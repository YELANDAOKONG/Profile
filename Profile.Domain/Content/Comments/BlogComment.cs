using System.Collections.ObjectModel;

using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Comments;

public sealed class BlogComment
{
    public const int MaximumBodyLength = CommentDomainRules.MaximumBodyLength;
    public const int MaximumMediaCount = CommentDomainRules.MaximumMediaCount;

    private ReadOnlyCollection<MediaReference> _media;

    private BlogComment(
        BlogCommentIdentity id,
        UserIdentity authorId,
        BlogIdentity blogId,
        BlogCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(blogId);

        if (parentCommentId == id)
        {
            throw new ArgumentException(
                "A Blog comment cannot be its own parent.",
                nameof(parentCommentId));
        }

        var copiedMedia = CommentDomainRules.CopyMedia(media);
        CommentDomainRules.ValidateState(body, copiedMedia, status);

        Id = id;
        AuthorId = authorId;
        BlogId = blogId;
        ParentCommentId = parentCommentId;
        Body = body;
        _media = copiedMedia;
        Status = status;
        CreatedAt = createdAt;
    }

    public BlogCommentIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public BlogIdentity BlogId { get; }

    public BlogCommentIdentity? ParentCommentId { get; }

    public ContentBody? Body { get; private set; }

    public IReadOnlyList<MediaReference> Media => _media;

    public CommentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsPlaceholder => Status is CommentStatus.Deleted;

    public static BlogComment Create(
        BlogCommentIdentity id,
        UserIdentity authorId,
        BlogIdentity blogId,
        BlogComment? parentComment,
        ContentBody body,
        IEnumerable<MediaReference> media,
        CommentModerationPolicy effectiveModerationPolicy,
        bool hasPreviouslyApprovedComment,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(body);

        EnsureValidParent(id, blogId, parentComment);

        var status = CommentModerationPolicyResolver.DetermineInitialStatus(
            effectiveModerationPolicy,
            hasPreviouslyApprovedComment);

        return new BlogComment(
            id,
            authorId,
            blogId,
            parentComment?.Id,
            body,
            media,
            status,
            createdAt);
    }

    public static BlogComment Reconstitute(
        BlogCommentIdentity id,
        UserIdentity authorId,
        BlogIdentity blogId,
        BlogCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt) =>
        new(
            id,
            authorId,
            blogId,
            parentCommentId,
            body,
            media,
            status,
            createdAt);

    public void Approve()
    {
        Status = CommentDomainRules.Approve(Status);
    }

    public void MarkAsSpam()
    {
        Status = CommentDomainRules.MarkAsSpam(Status);
    }

    public void Delete()
    {
        Status = CommentDomainRules.Delete(Status);
        Body = null;
        _media = Array.AsReadOnly(Array.Empty<MediaReference>());
    }

    private static void EnsureValidParent(
        BlogCommentIdentity id,
        BlogIdentity blogId,
        BlogComment? parentComment)
    {
        if (parentComment is null)
        {
            return;
        }

        if (parentComment.Id == id)
        {
            throw new ArgumentException(
                "A Blog comment cannot be its own parent.",
                nameof(parentComment));
        }

        if (parentComment.BlogId != blogId)
        {
            throw new ArgumentException(
                "A parent Blog comment must belong to the same Blog.",
                nameof(parentComment));
        }

        CommentDomainRules.EnsureCanReplyTo(parentComment.Status);
    }
}

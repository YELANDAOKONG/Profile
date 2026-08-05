using System.Collections.ObjectModel;

using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Comments;

public sealed class PostComment
{
    public const int MaximumBodyLength = CommentDomainRules.MaximumBodyLength;
    public const int MaximumMediaCount = CommentDomainRules.MaximumMediaCount;

    private ReadOnlyCollection<MediaReference> _media;

    private PostComment(
        PostCommentIdentity id,
        UserIdentity authorId,
        PostIdentity postId,
        PostCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(postId);

        if (parentCommentId == id)
        {
            throw new ArgumentException(
                "A Post comment cannot be its own parent.",
                nameof(parentCommentId));
        }

        var copiedMedia = CommentDomainRules.CopyMedia(media);
        CommentDomainRules.ValidateState(body, copiedMedia, status);

        Id = id;
        AuthorId = authorId;
        PostId = postId;
        ParentCommentId = parentCommentId;
        Body = body;
        _media = copiedMedia;
        Status = status;
        CreatedAt = createdAt;
    }

    public PostCommentIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public PostIdentity PostId { get; }

    public PostCommentIdentity? ParentCommentId { get; }

    public ContentBody? Body { get; private set; }

    public IReadOnlyList<MediaReference> Media => _media;

    public CommentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsPlaceholder => Status is CommentStatus.Deleted;

    public static PostComment Create(
        PostCommentIdentity id,
        UserIdentity authorId,
        PostIdentity postId,
        PostComment? parentComment,
        ContentBody body,
        IEnumerable<MediaReference> media,
        CommentModerationPolicy effectiveModerationPolicy,
        bool hasPreviouslyApprovedComment,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(body);

        EnsureValidParent(id, postId, parentComment);

        var status = CommentModerationPolicyResolver.DetermineInitialStatus(
            effectiveModerationPolicy,
            hasPreviouslyApprovedComment);

        return new PostComment(
            id,
            authorId,
            postId,
            parentComment?.Id,
            body,
            media,
            status,
            createdAt);
    }

    public static PostComment Reconstitute(
        PostCommentIdentity id,
        UserIdentity authorId,
        PostIdentity postId,
        PostCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt) =>
        new(
            id,
            authorId,
            postId,
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
        PostCommentIdentity id,
        PostIdentity postId,
        PostComment? parentComment)
    {
        if (parentComment is null)
        {
            return;
        }

        if (parentComment.Id == id)
        {
            throw new ArgumentException(
                "A Post comment cannot be its own parent.",
                nameof(parentComment));
        }

        if (parentComment.PostId != postId)
        {
            throw new ArgumentException(
                "A parent Post comment must belong to the same Post.",
                nameof(parentComment));
        }

        CommentDomainRules.EnsureCanReplyTo(parentComment.Status);
    }
}

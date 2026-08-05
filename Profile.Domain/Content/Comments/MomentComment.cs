using System.Collections.ObjectModel;

using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Comments;

public sealed class MomentComment
{
    public const int MaximumBodyLength = CommentDomainRules.MaximumBodyLength;
    public const int MaximumMediaCount = CommentDomainRules.MaximumMediaCount;

    private ReadOnlyCollection<MediaReference> _media;

    private MomentComment(
        MomentCommentIdentity id,
        UserIdentity authorId,
        MomentIdentity momentId,
        MomentCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(momentId);

        if (parentCommentId == id)
        {
            throw new ArgumentException(
                "A Moment comment cannot be its own parent.",
                nameof(parentCommentId));
        }

        var copiedMedia = CommentDomainRules.CopyMedia(media);
        CommentDomainRules.ValidateState(body, copiedMedia, status);

        Id = id;
        AuthorId = authorId;
        MomentId = momentId;
        ParentCommentId = parentCommentId;
        Body = body;
        _media = copiedMedia;
        Status = status;
        CreatedAt = createdAt;
    }

    public MomentCommentIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public MomentIdentity MomentId { get; }

    public MomentCommentIdentity? ParentCommentId { get; }

    public ContentBody? Body { get; private set; }

    public IReadOnlyList<MediaReference> Media => _media;

    public CommentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsPlaceholder => Status is CommentStatus.Deleted;

    public static MomentComment Create(
        MomentCommentIdentity id,
        UserIdentity authorId,
        MomentIdentity momentId,
        MomentComment? parentComment,
        ContentBody body,
        IEnumerable<MediaReference> media,
        CommentModerationPolicy effectiveModerationPolicy,
        bool hasPreviouslyApprovedComment,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(body);

        EnsureValidParent(id, momentId, parentComment);

        var status = CommentModerationPolicyResolver.DetermineInitialStatus(
            effectiveModerationPolicy,
            hasPreviouslyApprovedComment);

        return new MomentComment(
            id,
            authorId,
            momentId,
            parentComment?.Id,
            body,
            media,
            status,
            createdAt);
    }

    public static MomentComment Reconstitute(
        MomentCommentIdentity id,
        UserIdentity authorId,
        MomentIdentity momentId,
        MomentCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt) =>
        new(
            id,
            authorId,
            momentId,
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
        MomentCommentIdentity id,
        MomentIdentity momentId,
        MomentComment? parentComment)
    {
        if (parentComment is null)
        {
            return;
        }

        if (parentComment.Id == id)
        {
            throw new ArgumentException(
                "A Moment comment cannot be its own parent.",
                nameof(parentComment));
        }

        if (parentComment.MomentId != momentId)
        {
            throw new ArgumentException(
                "A parent Moment comment must belong to the same Moment.",
                nameof(parentComment));
        }

        CommentDomainRules.EnsureCanReplyTo(parentComment.Status);
    }
}

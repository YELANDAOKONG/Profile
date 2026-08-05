using System.Collections.ObjectModel;

using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Comments;

public sealed class PageComment
{
    public const int MaximumBodyLength = CommentDomainRules.MaximumBodyLength;
    public const int MaximumMediaCount = CommentDomainRules.MaximumMediaCount;

    private ReadOnlyCollection<MediaReference> _media;

    private PageComment(
        PageCommentIdentity id,
        UserIdentity authorId,
        PageIdentity pageId,
        PageCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(pageId);

        if (parentCommentId == id)
        {
            throw new ArgumentException(
                "A Page comment cannot be its own parent.",
                nameof(parentCommentId));
        }

        var copiedMedia = CommentDomainRules.CopyMedia(media);
        CommentDomainRules.ValidateState(body, copiedMedia, status);

        Id = id;
        AuthorId = authorId;
        PageId = pageId;
        ParentCommentId = parentCommentId;
        Body = body;
        _media = copiedMedia;
        Status = status;
        CreatedAt = createdAt;
    }

    public PageCommentIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public PageIdentity PageId { get; }

    public PageCommentIdentity? ParentCommentId { get; }

    public ContentBody? Body { get; private set; }

    public IReadOnlyList<MediaReference> Media => _media;

    public CommentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsPlaceholder => Status is CommentStatus.Deleted;

    public static PageComment Create(
        PageCommentIdentity id,
        UserIdentity authorId,
        PageIdentity pageId,
        PageComment? parentComment,
        ContentBody body,
        IEnumerable<MediaReference> media,
        CommentModerationPolicy effectiveModerationPolicy,
        bool hasPreviouslyApprovedComment,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(body);

        EnsureValidParent(id, pageId, parentComment);

        var status = CommentModerationPolicyResolver.DetermineInitialStatus(
            effectiveModerationPolicy,
            hasPreviouslyApprovedComment);

        return new PageComment(
            id,
            authorId,
            pageId,
            parentComment?.Id,
            body,
            media,
            status,
            createdAt);
    }

    public static PageComment Reconstitute(
        PageCommentIdentity id,
        UserIdentity authorId,
        PageIdentity pageId,
        PageCommentIdentity? parentCommentId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        CommentStatus status,
        DateTimeOffset createdAt) =>
        new(
            id,
            authorId,
            pageId,
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
        PageCommentIdentity id,
        PageIdentity pageId,
        PageComment? parentComment)
    {
        if (parentComment is null)
        {
            return;
        }

        if (parentComment.Id == id)
        {
            throw new ArgumentException(
                "A Page comment cannot be its own parent.",
                nameof(parentComment));
        }

        if (parentComment.PageId != pageId)
        {
            throw new ArgumentException(
                "A parent Page comment must belong to the same Page.",
                nameof(parentComment));
        }

        CommentDomainRules.EnsureCanReplyTo(parentComment.Status);
    }
}

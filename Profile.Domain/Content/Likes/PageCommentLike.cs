using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record PageCommentLike
{
    public PageCommentLike(
        UserIdentity likerId,
        PageCommentIdentity pageCommentId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(pageCommentId);

        LikerId = likerId;
        PageCommentId = pageCommentId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public PageCommentIdentity PageCommentId { get; }

    public DateTimeOffset LikedAt { get; }

    public static PageCommentLike Create(
        UserIdentity likerId,
        PageCommentIdentity pageCommentId,
        CommentStatus commentStatus,
        bool isHostActiveAndPublished,
        bool isInHostReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(pageCommentId);

        LikeEligibilityPolicy.EnsureCanLikeComment(
            commentStatus,
            isHostActiveAndPublished,
            isInHostReadingAudience,
            isBlockedBetweenAccounts);

        return new PageCommentLike(likerId, pageCommentId, likedAt);
    }
}

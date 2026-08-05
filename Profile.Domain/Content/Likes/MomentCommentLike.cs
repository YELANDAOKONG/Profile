using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record MomentCommentLike
{
    public MomentCommentLike(
        UserIdentity likerId,
        MomentCommentIdentity momentCommentId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(momentCommentId);

        LikerId = likerId;
        MomentCommentId = momentCommentId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public MomentCommentIdentity MomentCommentId { get; }

    public DateTimeOffset LikedAt { get; }

    public static MomentCommentLike Create(
        UserIdentity likerId,
        MomentCommentIdentity momentCommentId,
        CommentStatus commentStatus,
        bool isHostActiveAndPublished,
        bool isInHostReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(momentCommentId);

        LikeEligibilityPolicy.EnsureCanLikeComment(
            commentStatus,
            isHostActiveAndPublished,
            isInHostReadingAudience,
            isBlockedBetweenAccounts);

        return new MomentCommentLike(likerId, momentCommentId, likedAt);
    }
}

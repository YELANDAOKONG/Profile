using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record PostCommentLike
{
    public PostCommentLike(
        UserIdentity likerId,
        PostCommentIdentity postCommentId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(postCommentId);

        LikerId = likerId;
        PostCommentId = postCommentId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public PostCommentIdentity PostCommentId { get; }

    public DateTimeOffset LikedAt { get; }

    public static PostCommentLike Create(
        UserIdentity likerId,
        PostCommentIdentity postCommentId,
        CommentStatus commentStatus,
        bool isHostActiveAndPublished,
        bool isInHostReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(postCommentId);

        LikeEligibilityPolicy.EnsureCanLikeComment(
            commentStatus,
            isHostActiveAndPublished,
            isInHostReadingAudience,
            isBlockedBetweenAccounts);

        return new PostCommentLike(likerId, postCommentId, likedAt);
    }
}

using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record BlogCommentLike
{
    public BlogCommentLike(
        UserIdentity likerId,
        BlogCommentIdentity blogCommentId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(blogCommentId);

        LikerId = likerId;
        BlogCommentId = blogCommentId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public BlogCommentIdentity BlogCommentId { get; }

    public DateTimeOffset LikedAt { get; }

    public static BlogCommentLike Create(
        UserIdentity likerId,
        BlogCommentIdentity blogCommentId,
        CommentStatus commentStatus,
        bool isHostActiveAndPublished,
        bool isInHostReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(blogCommentId);

        LikeEligibilityPolicy.EnsureCanLikeComment(
            commentStatus,
            isHostActiveAndPublished,
            isInHostReadingAudience,
            isBlockedBetweenAccounts);

        return new BlogCommentLike(likerId, blogCommentId, likedAt);
    }
}

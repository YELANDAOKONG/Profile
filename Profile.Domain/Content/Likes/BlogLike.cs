using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record BlogLike
{
    public BlogLike(
        UserIdentity likerId,
        BlogIdentity blogId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(blogId);

        LikerId = likerId;
        BlogId = blogId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public BlogIdentity BlogId { get; }

    public DateTimeOffset LikedAt { get; }

    public static BlogLike Create(
        UserIdentity likerId,
        BlogIdentity blogId,
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(blogId);

        LikeEligibilityPolicy.EnsureCanLikeContent(
            isTargetActiveAndPublished,
            isInReadingAudience,
            isBlockedBetweenAccounts);

        return new BlogLike(likerId, blogId, likedAt);
    }
}

using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record PostLike
{
    public PostLike(
        UserIdentity likerId,
        PostIdentity postId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(postId);

        LikerId = likerId;
        PostId = postId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public PostIdentity PostId { get; }

    public DateTimeOffset LikedAt { get; }

    public static PostLike Create(
        UserIdentity likerId,
        PostIdentity postId,
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(postId);

        LikeEligibilityPolicy.EnsureCanLikeContent(
            isTargetActiveAndPublished,
            isInReadingAudience,
            isBlockedBetweenAccounts);

        return new PostLike(likerId, postId, likedAt);
    }
}

using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Likes;

public sealed record MomentLike
{
    public MomentLike(
        UserIdentity likerId,
        MomentIdentity momentId,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(momentId);

        LikerId = likerId;
        MomentId = momentId;
        LikedAt = likedAt;
    }

    public UserIdentity LikerId { get; }

    public MomentIdentity MomentId { get; }

    public DateTimeOffset LikedAt { get; }

    public static MomentLike Create(
        UserIdentity likerId,
        MomentIdentity momentId,
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset likedAt)
    {
        ArgumentNullException.ThrowIfNull(likerId);
        ArgumentNullException.ThrowIfNull(momentId);

        LikeEligibilityPolicy.EnsureCanLikeContent(
            isTargetActiveAndPublished,
            isInReadingAudience,
            isBlockedBetweenAccounts);

        return new MomentLike(likerId, momentId, likedAt);
    }
}

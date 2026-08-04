using Profile.Domain.Users.Value;

namespace Profile.Domain.Social;

public sealed record Follow
{
    public Follow(
        UserIdentity followerId,
        UserIdentity followedId,
        DateTimeOffset followedAt)
    {
        ArgumentNullException.ThrowIfNull(followerId);
        ArgumentNullException.ThrowIfNull(followedId);

        if (followerId == followedId)
        {
            throw new ArgumentException(
                "An account cannot follow itself.",
                nameof(followedId));
        }

        FollowerId = followerId;
        FollowedId = followedId;
        FollowedAt = followedAt;
    }

    public UserIdentity FollowerId { get; }

    public UserIdentity FollowedId { get; }

    public DateTimeOffset FollowedAt { get; }

    public static Follow Create(
        UserIdentity followerId,
        UserIdentity followedId,
        DateTimeOffset followedAt) =>
        new(followerId, followedId, followedAt);
}

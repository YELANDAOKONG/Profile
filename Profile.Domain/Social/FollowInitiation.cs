using Profile.Domain.Users.Value;

namespace Profile.Domain.Social;

public sealed class FollowInitiation
{
    private FollowInitiation(Follow? follow, FollowRequest? request)
    {
        if ((follow is null) == (request is null))
        {
            throw new ArgumentException(
                "A follow initiation must contain exactly one result.",
                nameof(request));
        }

        CreatedFollow = follow;
        CreatedRequest = request;
    }

    public Follow? CreatedFollow { get; }

    public FollowRequest? CreatedRequest { get; }

    public bool IsPendingApproval => CreatedRequest is not null;

    public static FollowInitiation Start(
        UserIdentity followerId,
        UserIdentity followedId,
        bool requiresApproval,
        bool isBlocked,
        DateTimeOffset initiatedAt)
    {
        ArgumentNullException.ThrowIfNull(followerId);
        ArgumentNullException.ThrowIfNull(followedId);

        if (followerId == followedId)
        {
            throw new ArgumentException(
                "An account cannot follow itself.",
                nameof(followedId));
        }

        if (isBlocked)
        {
            throw new InvalidOperationException(
                "An account cannot follow another account while either account blocks the other.");
        }

        return requiresApproval
            ? new FollowInitiation(
                null,
                FollowRequest.Create(followerId, followedId, initiatedAt))
            : new FollowInitiation(
                Follow.Create(
                    followerId,
                    followedId,
                    initiatedAt),
                null);
    }
}

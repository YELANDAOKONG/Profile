using Profile.Domain.Users.Value;

namespace Profile.Domain.Social;

public sealed record AccountBlock
{
    public AccountBlock(
        UserIdentity blockerId,
        UserIdentity blockedId,
        DateTimeOffset blockedAt)
    {
        ArgumentNullException.ThrowIfNull(blockerId);
        ArgumentNullException.ThrowIfNull(blockedId);

        if (blockerId == blockedId)
        {
            throw new ArgumentException(
                "An account cannot block itself.",
                nameof(blockedId));
        }

        BlockerId = blockerId;
        BlockedId = blockedId;
        BlockedAt = blockedAt;
    }

    public UserIdentity BlockerId { get; }

    public UserIdentity BlockedId { get; }

    public DateTimeOffset BlockedAt { get; }

    public static AccountBlock Create(
        UserIdentity blockerId,
        UserIdentity blockedId,
        DateTimeOffset blockedAt) =>
        new(blockerId, blockedId, blockedAt);

    public bool Invalidates(Follow follow)
    {
        ArgumentNullException.ThrowIfNull(follow);

        return AppliesTo(follow.FollowerId, follow.FollowedId);
    }

    public bool Invalidates(FollowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.IsPending && AppliesTo(request.RequesterId, request.TargetId);
    }

    public bool AppliesTo(UserIdentity firstAccountId, UserIdentity secondAccountId)
    {
        ArgumentNullException.ThrowIfNull(firstAccountId);
        ArgumentNullException.ThrowIfNull(secondAccountId);

        return
            (firstAccountId == BlockerId && secondAccountId == BlockedId) ||
            (firstAccountId == BlockedId && secondAccountId == BlockerId);
    }
}

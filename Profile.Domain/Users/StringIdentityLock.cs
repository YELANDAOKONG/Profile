using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed record StringIdentityLock
{
    public StringIdentityLock(
        UserIdentity ownerId,
        StringIdentity stringId,
        DateTimeOffset lockedAt)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(stringId);

        OwnerId = ownerId;
        StringId = stringId;
        LockedAt = lockedAt;
    }

    public UserIdentity OwnerId { get; }

    public StringIdentity StringId { get; }

    public DateTimeOffset LockedAt { get; }
}

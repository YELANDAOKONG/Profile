using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class StringIdentityLockTests
{
    [Fact]
    public void Constructor_WithCompleteState_PreservesValues()
    {
        var ownerId = UserIdentity.New();
        var stringId = new StringIdentity("account.name");
        var lockedAt = new DateTimeOffset(
            2026,
            1,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

        var identityLock = new StringIdentityLock(
            ownerId,
            stringId,
            lockedAt);

        Assert.Equal(ownerId, identityLock.OwnerId);
        Assert.Equal(stringId, identityLock.StringId);
        Assert.Equal(lockedAt, identityLock.LockedAt);
    }
}

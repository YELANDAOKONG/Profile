using Profile.Domain.Social;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Social;

public sealed class FollowTests
{
    private static readonly DateTimeOffset _followedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithDistinctAccounts_PreservesRelationship()
    {
        var followerId = UserIdentity.New();
        var followedId = UserIdentity.New();

        var follow = Follow.Create(followerId, followedId, _followedAt);

        Assert.Equal(followerId, follow.FollowerId);
        Assert.Equal(followedId, follow.FollowedId);
        Assert.Equal(_followedAt, follow.FollowedAt);
    }

    [Fact]
    public void Create_WithSameAccount_ThrowsArgumentException()
    {
        var accountId = UserIdentity.New();

        var exception = Assert.Throws<ArgumentException>(
            () => Follow.Create(accountId, accountId, _followedAt));

        Assert.Equal("followedId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullFollower_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Follow(null!, UserIdentity.New(), _followedAt));
    }

    [Fact]
    public void Constructor_WithNullFollowedAccount_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Follow(UserIdentity.New(), null!, _followedAt));
    }
}

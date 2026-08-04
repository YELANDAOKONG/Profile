using Profile.Domain.Social;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Social;

public sealed class FollowInitiationTests
{
    private static readonly DateTimeOffset _initiatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_WithoutApproval_CreatesDirectFollow()
    {
        var followerId = UserIdentity.New();
        var followedId = UserIdentity.New();

        var initiation = FollowInitiation.Start(
            followerId,
            followedId,
            false,
            false,
            _initiatedAt);

        Assert.False(initiation.IsPendingApproval);
        Assert.Null(initiation.CreatedRequest);
        Assert.Equal(followerId, initiation.CreatedFollow?.FollowerId);
        Assert.Equal(followedId, initiation.CreatedFollow?.FollowedId);
        Assert.Equal(_initiatedAt, initiation.CreatedFollow?.FollowedAt);
    }

    [Fact]
    public void Start_WithApproval_CreatesPendingRequest()
    {
        var followerId = UserIdentity.New();
        var followedId = UserIdentity.New();

        var initiation = FollowInitiation.Start(
            followerId,
            followedId,
            true,
            false,
            _initiatedAt);

        Assert.True(initiation.IsPendingApproval);
        Assert.Null(initiation.CreatedFollow);
        Assert.True(initiation.CreatedRequest?.IsPending);
        Assert.Equal(followerId, initiation.CreatedRequest?.RequesterId);
        Assert.Equal(followedId, initiation.CreatedRequest?.TargetId);
    }

    [Fact]
    public void Start_WhileEitherDirectionIsBlocked_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => FollowInitiation.Start(
                UserIdentity.New(),
                UserIdentity.New(),
                false,
                true,
                _initiatedAt));
    }

    [Fact]
    public void Start_WithSameAccount_ThrowsArgumentException()
    {
        var accountId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => FollowInitiation.Start(
                accountId,
                accountId,
                false,
                false,
                _initiatedAt));
    }

    [Fact]
    public void Start_WithNullAccount_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => FollowInitiation.Start(
                null!,
                UserIdentity.New(),
                false,
                false,
                _initiatedAt));
    }
}

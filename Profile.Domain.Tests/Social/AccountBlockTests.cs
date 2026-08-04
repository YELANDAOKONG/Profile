using Profile.Domain.Social;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Social;

public sealed class AccountBlockTests
{
    private static readonly DateTimeOffset _blockedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithDistinctAccounts_PreservesRelationship()
    {
        var blockerId = UserIdentity.New();
        var blockedId = UserIdentity.New();

        var block = AccountBlock.Create(blockerId, blockedId, _blockedAt);

        Assert.Equal(blockerId, block.BlockerId);
        Assert.Equal(blockedId, block.BlockedId);
        Assert.Equal(_blockedAt, block.BlockedAt);
    }

    [Fact]
    public void Create_WithSameAccount_ThrowsArgumentException()
    {
        var accountId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => AccountBlock.Create(accountId, accountId, _blockedAt));
    }

    [Fact]
    public void AppliesTo_MatchesThePairInEitherDirection()
    {
        var blockerId = UserIdentity.New();
        var blockedId = UserIdentity.New();
        var block = AccountBlock.Create(blockerId, blockedId, _blockedAt);

        Assert.True(block.AppliesTo(blockerId, blockedId));
        Assert.True(block.AppliesTo(blockedId, blockerId));
    }

    [Fact]
    public void AppliesTo_WithUnrelatedAccount_ReturnsFalse()
    {
        var block = AccountBlock.Create(
            UserIdentity.New(),
            UserIdentity.New(),
            _blockedAt);

        Assert.False(block.AppliesTo(UserIdentity.New(), UserIdentity.New()));
    }

    [Fact]
    public void Invalidates_FollowsInBothDirections()
    {
        var blockerId = UserIdentity.New();
        var blockedId = UserIdentity.New();
        var block = AccountBlock.Create(blockerId, blockedId, _blockedAt);
        var outgoing = Follow.Create(blockerId, blockedId, _blockedAt);
        var incoming = Follow.Create(blockedId, blockerId, _blockedAt);

        Assert.True(block.Invalidates(outgoing));
        Assert.True(block.Invalidates(incoming));
    }

    [Fact]
    public void Invalidates_PendingRequestsInBothDirections()
    {
        var blockerId = UserIdentity.New();
        var blockedId = UserIdentity.New();
        var block = AccountBlock.Create(blockerId, blockedId, _blockedAt);
        var outgoing = FollowRequest.Create(blockerId, blockedId, _blockedAt);
        var incoming = FollowRequest.Create(blockedId, blockerId, _blockedAt);

        Assert.True(block.Invalidates(outgoing));
        Assert.True(block.Invalidates(incoming));
    }

    [Fact]
    public void Invalidates_ResolvedRequest_ReturnsFalse()
    {
        var blockerId = UserIdentity.New();
        var blockedId = UserIdentity.New();
        var block = AccountBlock.Create(blockerId, blockedId, _blockedAt);
        var request = FollowRequest.Create(blockerId, blockedId, _blockedAt);
        request.Reject(blockedId, _blockedAt);

        Assert.False(block.Invalidates(request));
    }

    [Fact]
    public void Invalidates_UnrelatedRelationship_ReturnsFalse()
    {
        var block = AccountBlock.Create(
            UserIdentity.New(),
            UserIdentity.New(),
            _blockedAt);
        var follow = Follow.Create(
            UserIdentity.New(),
            UserIdentity.New(),
            _blockedAt);

        Assert.False(block.Invalidates(follow));
    }
}

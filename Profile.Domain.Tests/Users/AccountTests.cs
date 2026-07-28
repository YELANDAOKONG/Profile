using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountTests
{
    [Fact]
    public void Initialization_WithIndependentAccountStates_PreservesData()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddMinutes(4);
        var suspendedAt = createdAt.AddMinutes(2);
        var bannedAt = createdAt.AddMinutes(3);
        var deletionRequestedAt = createdAt.AddMinutes(4);
        var recoveryEndsAt = deletionRequestedAt.AddDays(14);
        var suspension = new AccountSuspension(suspendedAt, null, "Maintenance required.");
        var ban = new AccountBan(bannedAt, bannedAt.AddDays(1));
        var deletion = new AccountDeletion(deletionRequestedAt, recoveryEndsAt);

        var account = Account.Reconstitute(
            new UserIdentity(Guid.NewGuid()),
            new StringIdentity("account.name"),
            new AccountEmail(new EmailAddress("user@example.com"), null),
            AccountRole.Root,
            createdAt,
            updatedAt,
            suspension,
            ban,
            deletion);

        Assert.Equal(AccountRole.Root, account.Role);
        Assert.Equal(suspension, account.Suspension);
        Assert.Equal(ban, account.Ban);
        Assert.Equal("Maintenance required.", account.Suspension?.Reason);
        Assert.Null(account.Ban?.Reason);
        Assert.Equal(deletion, account.Deletion);
    }

    [Fact]
    public void Initialization_WithoutRestrictions_LeavesStatesEmpty()
    {
        var now = DateTimeOffset.UtcNow;

        var account = Account.Create(
            new UserIdentity(Guid.NewGuid()),
            new StringIdentity("account.name"),
            new AccountEmail(new EmailAddress("user@example.com"), null),
            AccountRole.User,
            now);

        Assert.Null(account.Suspension);
        Assert.Null(account.Ban);
        Assert.Null(account.Deletion);
    }
}

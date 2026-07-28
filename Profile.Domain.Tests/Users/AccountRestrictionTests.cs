using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountRestrictionTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Suspend_WithHigherRole_KeepsLoginButBlocksOperationsUntilExpiration()
    {
        var administrator = CreateAccount(AccountRole.Administrator, "admin.account");
        var account = CreateAccount(AccountRole.User, "user.account");
        var suspendedAt = _createdAt.AddMinutes(1);
        var expiresAt = suspendedAt.AddHours(1);

        account.Suspend(
            administrator,
            suspendedAt,
            expiresAt,
            "Maintenance required.");

        Assert.Equal("Maintenance required.", account.Suspension?.Reason);
        Assert.True(account.IsSuspendedAt(suspendedAt));
        Assert.True(account.CanLoginAt(suspendedAt));
        Assert.False(account.CanPerformOperationsAt(suspendedAt));
        Assert.False(account.IsSuspendedAt(expiresAt));
        Assert.True(account.CanPerformOperationsAt(expiresAt));
    }

    [Fact]
    public void BanAccount_WithHigherRole_BlocksLoginUntilExpiration()
    {
        var administrator = CreateAccount(AccountRole.Administrator, "admin.account");
        var account = CreateAccount(AccountRole.User, "user.account");
        var bannedAt = _createdAt.AddMinutes(1);
        var expiresAt = bannedAt.AddHours(1);

        account.BanAccount(
            administrator,
            bannedAt,
            expiresAt,
            "Policy violation.");

        Assert.Equal("Policy violation.", account.Ban?.Reason);
        Assert.True(account.IsBannedAt(bannedAt));
        Assert.False(account.CanLoginAt(bannedAt));
        Assert.False(account.CanPerformOperationsAt(bannedAt));
        Assert.False(account.IsBannedAt(expiresAt));
        Assert.True(account.CanLoginAt(expiresAt));
    }

    [Fact]
    public void Restriction_WithExpirationAtStart_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AccountSuspension(_createdAt, _createdAt));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AccountBan(_createdAt, _createdAt));
    }

    [Fact]
    public void SystemAdministration_CanRestrictAndReleaseRootAccount()
    {
        var account = CreateAccount(AccountRole.Root, "root.account");
        var suspendedAt = _createdAt.AddMinutes(1);
        var bannedAt = _createdAt.AddMinutes(2);
        var suspensionRemovedAt = _createdAt.AddMinutes(3);
        var banRemovedAt = _createdAt.AddMinutes(4);

        account.SuspendBySystemAdministration(suspendedAt);
        account.BanBySystemAdministration(bannedAt);

        Assert.True(account.IsSuspendedAt(bannedAt));
        Assert.True(account.IsBannedAt(bannedAt));

        account.RemoveSuspensionBySystemAdministration(suspensionRemovedAt);
        account.RemoveBanBySystemAdministration(banRemovedAt);

        Assert.Null(account.Suspension);
        Assert.Null(account.Ban);
        Assert.True(account.CanPerformOperationsAt(banRemovedAt));
    }

    private static Account CreateAccount(
        AccountRole role,
        string stringId) =>
        Account.Create(
            new UserIdentity(Guid.NewGuid()),
            new StringIdentity(stringId),
            new AccountEmail(new EmailAddress("user@example.com"), null),
            role,
            _createdAt);
}

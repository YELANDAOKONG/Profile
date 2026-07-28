using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountAdministrationTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CanManage_UsesStrictRoleHierarchy()
    {
        var user = CreateAccount(AccountRole.User, "user.account");
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var otherAdministrator = CreateAccount(
            AccountRole.Administrator,
            "other.admin");
        var root = CreateAccount(AccountRole.Root, "root.account");
        var otherRoot = CreateAccount(AccountRole.Root, "other.root");

        Assert.True(administrator.CanManage(user, _createdAt));
        Assert.False(administrator.CanManage(otherAdministrator, _createdAt));
        Assert.False(administrator.CanManage(root, _createdAt));
        Assert.True(root.CanManage(user, _createdAt));
        Assert.True(root.CanManage(administrator, _createdAt));
        Assert.False(root.CanManage(otherRoot, _createdAt));
    }

    [Fact]
    public void CanManage_BeforeTargetAccountExists_ReturnsFalse()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var targetCreatedAt = _createdAt.AddMinutes(1);
        var target = Account.Create(
            UserIdentity.New(),
            new StringIdentity("user.account"),
            new AccountEmail(new EmailAddress("user@example.com"), null),
            AccountRole.User,
            targetCreatedAt);

        Assert.False(administrator.CanManage(target, _createdAt));
    }

    [Fact]
    public void Suspend_WithSameRoleAdministrator_ThrowsInvalidOperationException()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var target = CreateAccount(
            AccountRole.Administrator,
            "target.admin");

        Assert.Throws<InvalidOperationException>(
            () => target.Suspend(
                administrator,
                _createdAt.AddMinutes(1)));
    }

    [Fact]
    public void Root_CannotAdministrativelyRestrictAnotherRoot()
    {
        var root = CreateAccount(AccountRole.Root, "root.account");
        var target = CreateAccount(AccountRole.Root, "target.root");
        var changedAt = _createdAt.AddMinutes(1);

        Assert.Throws<InvalidOperationException>(
            () => target.Suspend(root, changedAt));
        Assert.Throws<InvalidOperationException>(
            () => target.BanAccount(root, changedAt));
    }

    [Fact]
    public void SuspendedAdministrator_CannotManageLowerAccount()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var target = CreateAccount(AccountRole.User, "user.account");
        var suspendedAt = _createdAt.AddMinutes(1);
        administrator.SuspendBySystemAdministration(suspendedAt);

        Assert.Throws<InvalidOperationException>(
            () => target.BanAccount(
                administrator,
                suspendedAt.AddMinutes(1)));
    }

    [Fact]
    public void ChangeRole_WithRoot_PromotesAndDemotesAdministrator()
    {
        var root = CreateAccount(AccountRole.Root, "root.account");
        var target = CreateAccount(AccountRole.User, "user.account");
        var promotedAt = _createdAt.AddMinutes(1);
        var demotedAt = _createdAt.AddMinutes(2);

        target.ChangeRole(root, AccountRole.Administrator, promotedAt);

        Assert.Equal(AccountRole.Administrator, target.Role);

        target.ChangeRole(root, AccountRole.User, demotedAt);

        Assert.Equal(AccountRole.User, target.Role);
        Assert.Equal(demotedAt, target.UpdatedAt);
    }

    [Fact]
    public void ChangeRole_WithAdministrator_ThrowsInvalidOperationException()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var target = CreateAccount(AccountRole.User, "user.account");

        Assert.Throws<InvalidOperationException>(
            () => target.ChangeRole(
                administrator,
                AccountRole.Administrator,
                _createdAt.AddMinutes(1)));
    }

    [Fact]
    public void ChangeRole_ToRoot_ThrowsInvalidOperationException()
    {
        var root = CreateAccount(AccountRole.Root, "root.account");
        var target = CreateAccount(AccountRole.User, "user.account");

        Assert.Throws<InvalidOperationException>(
            () => target.ChangeRole(
                root,
                AccountRole.Root,
                _createdAt.AddMinutes(1)));
    }

    [Fact]
    public void ChangeRoleBySystemAdministration_CanGrantAndRevokeRoot()
    {
        var account = CreateAccount(AccountRole.User, "user.account");
        var promotedAt = _createdAt.AddMinutes(1);
        var demotedAt = _createdAt.AddMinutes(2);

        account.ChangeRoleBySystemAdministration(
            AccountRole.Root,
            promotedAt);

        Assert.Equal(AccountRole.Root, account.Role);

        account.ChangeRoleBySystemAdministration(
            AccountRole.User,
            demotedAt);

        Assert.Equal(AccountRole.User, account.Role);
        Assert.Equal(demotedAt, account.UpdatedAt);
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

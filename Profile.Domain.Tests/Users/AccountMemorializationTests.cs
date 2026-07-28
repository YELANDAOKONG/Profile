using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountMemorializationTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _recoveryPeriod = TimeSpan.FromDays(14);

    [Fact]
    public void Memorialize_WithHigherRole_MakesAccountTerminal()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var account = CreateAccount(AccountRole.User, "user.account");
        var memorializedAt = _createdAt.AddMinutes(1);

        account.Memorialize(administrator, memorializedAt);

        Assert.Equal(memorializedAt, account.Memorialization?.MemorializedAt);
        Assert.Equal(memorializedAt, account.UpdatedAt);
        Assert.True(account.IsMemorializedAt(memorializedAt));
        Assert.False(account.CanLoginAt(memorializedAt));
        Assert.False(account.CanPerformOperationsAt(memorializedAt));
    }

    [Fact]
    public void Memorialize_WithPendingDeletion_ThrowsInvalidOperationException()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var account = CreateAccount(AccountRole.User, "user.account");
        var deletionRequestedAt = _createdAt.AddMinutes(1);
        account.RequestDeletion(
            deletionRequestedAt,
            _recoveryPeriod,
            AccountDeletionContentPolicy.PreserveVisibility);

        Assert.Throws<InvalidOperationException>(
            () => account.Memorialize(
                administrator,
                deletionRequestedAt.AddMinutes(1)));
    }

    [Fact]
    public void Memorialize_WhileRestricted_PreservesRestrictions()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var account = CreateAccount(AccountRole.User, "user.account");
        var restrictedAt = _createdAt.AddMinutes(1);
        var memorializedAt = _createdAt.AddMinutes(2);
        account.SuspendBySystemAdministration(restrictedAt);
        account.BanBySystemAdministration(restrictedAt);

        account.Memorialize(administrator, memorializedAt);

        Assert.NotNull(account.Suspension);
        Assert.NotNull(account.Ban);
        Assert.NotNull(account.Memorialization);
    }

    [Fact]
    public void Memorialize_WithoutHigherRole_ThrowsInvalidOperationException()
    {
        var administrator = CreateAccount(AccountRole.User, "other.user");
        var account = CreateAccount(AccountRole.User, "user.account");

        Assert.Throws<InvalidOperationException>(
            () => account.Memorialize(
                administrator,
                _createdAt.AddMinutes(1)));
    }

    [Fact]
    public void MemorializedAccount_CannotChangeIdentity()
    {
        var administrator = CreateAccount(
            AccountRole.Administrator,
            "admin.account");
        var account = CreateAccount(AccountRole.User, "user.account");
        var memorializedAt = _createdAt.AddMinutes(1);
        account.Memorialize(administrator, memorializedAt);

        Assert.Throws<InvalidOperationException>(
            () => account.ChangeStringId(
                new StringIdentity("changed.account"),
                memorializedAt.AddMinutes(1)));
    }

    [Fact]
    public void SystemAdministration_CanMemorializeAndManageRootAccount()
    {
        var account = CreateAccount(AccountRole.Root, "root.account");
        var restrictedAt = _createdAt.AddMinutes(1);
        var memorializedAt = _createdAt.AddMinutes(2);
        var changedAt = _createdAt.AddMinutes(3);
        account.SuspendBySystemAdministration(restrictedAt);
        account.BanBySystemAdministration(restrictedAt);

        account.MemorializeBySystemAdministration(memorializedAt);
        account.RemoveSuspensionBySystemAdministration(changedAt);
        account.RemoveBanBySystemAdministration(changedAt);
        account.ChangeRoleBySystemAdministration(AccountRole.User, changedAt);

        Assert.NotNull(account.Memorialization);
        Assert.Null(account.Suspension);
        Assert.Null(account.Ban);
        Assert.Equal(AccountRole.User, account.Role);
        Assert.False(account.CanLoginAt(changedAt));
        Assert.False(account.CanPerformOperationsAt(changedAt));
    }

    [Fact]
    public void Reconstitute_WithDeletionAndMemorialization_ThrowsArgumentException()
    {
        var updatedAt = _createdAt.AddMinutes(1);
        var deletion = new AccountDeletion(
            updatedAt,
            updatedAt.Add(_recoveryPeriod),
            AccountDeletionContentPolicy.PreserveVisibility);
        var memorialization = new AccountMemorialization(updatedAt);

        Assert.Throws<ArgumentException>(
            () => Account.Reconstitute(
                UserIdentity.New(),
                new StringIdentity("user.account"),
                new AccountEmail(new EmailAddress("user@example.com"), null),
                AccountRole.User,
                _createdAt,
                updatedAt,
                null,
                null,
                deletion,
                memorialization));
    }

    private static Account CreateAccount(
        AccountRole role,
        string stringId) =>
        Account.Create(
            UserIdentity.New(),
            new StringIdentity(stringId),
            new AccountEmail(new EmailAddress("user@example.com"), null),
            role,
            _createdAt);
}

using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountDeletionTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _recoveryPeriod = TimeSpan.FromDays(14);

    [Fact]
    public void RequestDeletion_WithRecoveryPeriod_AllowsOnlyRecoveryLogin()
    {
        var account = CreateAccount();
        var requestedAt = _createdAt.AddMinutes(1);
        var recoveryEndsAt = requestedAt.Add(_recoveryPeriod);

        account.RequestDeletion(requestedAt, _recoveryPeriod);

        Assert.Equal(requestedAt, account.Deletion?.RequestedAt);
        Assert.Equal(recoveryEndsAt, account.Deletion?.RecoveryEndsAt);
        Assert.True(account.IsDeletionPendingAt(requestedAt));
        Assert.True(account.CanLoginAt(requestedAt));
        Assert.True(account.CanRestoreAt(requestedAt));
        Assert.False(account.CanPerformOperationsAt(requestedAt));
    }

    [Fact]
    public void Restore_WhileSuspended_RestoresDeletionAndKeepsSuspension()
    {
        var account = CreateAccount();
        var requestedAt = _createdAt.AddMinutes(1);
        var suspendedAt = _createdAt.AddMinutes(2);
        var restoredAt = _createdAt.AddMinutes(3);
        account.RequestDeletion(requestedAt, _recoveryPeriod);
        account.SuspendBySystemAdministration(suspendedAt);

        account.Restore(restoredAt);

        Assert.Null(account.Deletion);
        Assert.True(account.IsSuspendedAt(restoredAt));
        Assert.True(account.CanLoginAt(restoredAt));
        Assert.False(account.CanPerformOperationsAt(restoredAt));
    }

    [Fact]
    public void Restore_WhileBanned_ThrowsInvalidOperationException()
    {
        var account = CreateAccount();
        var requestedAt = _createdAt.AddMinutes(1);
        var bannedAt = _createdAt.AddMinutes(2);
        var restoredAt = _createdAt.AddMinutes(3);
        account.RequestDeletion(requestedAt, _recoveryPeriod);
        account.BanBySystemAdministration(bannedAt);

        Assert.False(account.CanLoginAt(restoredAt));
        Assert.False(account.CanRestoreAt(restoredAt));
        Assert.Throws<InvalidOperationException>(
            () => account.Restore(restoredAt));
    }

    [Fact]
    public void RecoveryEnd_BlocksLoginAndMarksAccountReadyForPermanentDeletion()
    {
        var account = CreateAccount();
        var requestedAt = _createdAt.AddMinutes(1);
        var recoveryEndsAt = requestedAt.Add(_recoveryPeriod);
        account.RequestDeletion(requestedAt, _recoveryPeriod);

        Assert.False(account.IsDeletionPendingAt(recoveryEndsAt));
        Assert.False(account.CanLoginAt(recoveryEndsAt));
        Assert.False(account.CanRestoreAt(recoveryEndsAt));
        Assert.True(account.IsReadyForPermanentDeletionAt(recoveryEndsAt));
        Assert.Throws<InvalidOperationException>(
            () => account.Restore(recoveryEndsAt));
    }

    [Fact]
    public void RequestDeletion_WithNonPositiveRecoveryPeriod_ThrowsArgumentOutOfRangeException()
    {
        var account = CreateAccount();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => account.RequestDeletion(
                _createdAt.AddMinutes(1),
                TimeSpan.Zero));
    }

    [Fact]
    public void AccountDeletion_WithInvalidRecoveryEnd_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AccountDeletion(_createdAt, _createdAt));
    }

    private static Account CreateAccount() =>
        Account.Create(
            new UserIdentity(Guid.NewGuid()),
            new StringIdentity("user.account"),
            new AccountEmail(new EmailAddress("user@example.com"), null),
            AccountRole.User,
            _createdAt);
}

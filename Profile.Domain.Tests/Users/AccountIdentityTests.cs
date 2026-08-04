using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountIdentityTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ChangeStringId_WhenAccountCanOperate_ChangesCurrentIdentity()
    {
        var account = CreateAccount();
        var changedAt = _createdAt.AddMinutes(1);
        var stringId = new StringIdentity("new.account");

        var reservation = account.ChangeStringId(stringId, changedAt);

        Assert.Equal(stringId, account.StringId);
        Assert.Equal(changedAt, account.UpdatedAt);
        Assert.NotNull(reservation);
        Assert.Equal(new StringIdentity("account.name"), reservation.StringId);
        Assert.Equal(account.Id, reservation.PreviousOwnerId);
        Assert.Equal(changedAt, reservation.ReservedAt);
        Assert.Equal(
            changedAt.AddDays(StringIdentityReservation.DefaultReservationPeriodDays),
            reservation.ReleasesAt);
    }

    [Fact]
    public void ChangeStringId_WithCustomPeriod_FixesReservationReleaseTime()
    {
        const int reservationPeriodDays = 45;

        var account = CreateAccount();
        var changedAt = _createdAt.AddMinutes(1);
        var period = TimeSpan.FromDays(reservationPeriodDays);

        var reservation = account.ChangeStringId(
            new StringIdentity("new.account"),
            changedAt,
            period);

        Assert.Equal(changedAt.Add(period), reservation?.ReleasesAt);
    }

    [Fact]
    public void ChangeStringId_WithCasingOnlyChange_DoesNotCreateReservation()
    {
        var account = CreateAccount();
        var changedAt = _createdAt.AddMinutes(1);

        var reservation = account.ChangeStringId(
            new StringIdentity("ACCOUNT.NAME"),
            changedAt);

        Assert.Null(reservation);
        Assert.Equal("ACCOUNT.NAME", account.StringId.Value);
        Assert.Equal(changedAt, account.UpdatedAt);
    }

    [Fact]
    public void ChangeStringId_WithExactCurrentValue_DoesNotCreateReservationOrUpdateTime()
    {
        var account = CreateAccount();

        var reservation = account.ChangeStringId(
            new StringIdentity("account.name"),
            _createdAt.AddMinutes(1));

        Assert.Null(reservation);
        Assert.Equal(_createdAt, account.UpdatedAt);
    }

    [Fact]
    public void ChangeStringId_WithNonPositiveReservationPeriod_DoesNotChangeAccount()
    {
        var account = CreateAccount();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => account.ChangeStringId(
                new StringIdentity("new.account"),
                _createdAt.AddMinutes(1),
                TimeSpan.Zero));

        Assert.Equal(new StringIdentity("account.name"), account.StringId);
        Assert.Equal(_createdAt, account.UpdatedAt);
    }

    [Fact]
    public void ChangeEmail_WithDifferentAddress_ClearsVerification()
    {
        var account = CreateAccount(verifiedAt: _createdAt);
        var changedAt = _createdAt.AddMinutes(1);
        var address = new EmailAddress("changed@example.com");

        account.ChangeEmail(address, changedAt);

        Assert.Equal(address, account.Email.Address);
        Assert.Null(account.Email.VerifiedAt);
    }

    [Fact]
    public void ChangeEmail_WithCasingOnlyChange_PreservesVerification()
    {
        var account = CreateAccount(
            address: "User@Example.com",
            verifiedAt: _createdAt);
        var address = new EmailAddress("user@example.com");

        account.ChangeEmail(address, _createdAt.AddMinutes(1));

        Assert.Equal(address.Value, account.Email.Address.Value);
        Assert.Equal(_createdAt, account.Email.VerifiedAt);
    }

    [Fact]
    public void VerifyEmail_WhenAccountCanOperate_StoresVerificationTime()
    {
        var account = CreateAccount();
        var verifiedAt = _createdAt.AddMinutes(1);

        account.VerifyEmail(verifiedAt);

        Assert.Equal(verifiedAt, account.Email.VerifiedAt);
        Assert.Equal(verifiedAt, account.UpdatedAt);
    }

    [Fact]
    public void ChangeStringId_WhileSuspended_ThrowsInvalidOperationException()
    {
        var account = CreateAccount();
        var suspendedAt = _createdAt.AddMinutes(1);
        account.SuspendBySystemAdministration(suspendedAt);

        Assert.Throws<InvalidOperationException>(
            () => account.ChangeStringId(
                new StringIdentity("new.account"),
                suspendedAt.AddMinutes(1)));
    }

    private static Account CreateAccount(
        string address = "user@example.com",
        DateTimeOffset? verifiedAt = null) =>
        Account.Create(
            new UserIdentity(Guid.NewGuid()),
            new StringIdentity("account.name"),
            new AccountEmail(new EmailAddress(address), verifiedAt),
            AccountRole.User,
            _createdAt);
}

using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class StringIdentityReservationTests
{
    private const int _reservationPeriodDays = 30;

    private static readonly DateTimeOffset _reservedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidPeriod_FixesReleaseTime()
    {
        var period = TimeSpan.FromDays(_reservationPeriodDays);

        var reservation = StringIdentityReservation.Create(
            UserIdentity.New(),
            new StringIdentity("account.name"),
            _reservedAt,
            period);

        Assert.Equal(_reservedAt.Add(period), reservation.ReleasesAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePeriod_ThrowsArgumentOutOfRangeException(
        int periodDays)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StringIdentityReservation.Create(
                UserIdentity.New(),
                new StringIdentity("account.name"),
                _reservedAt,
                TimeSpan.FromDays(periodDays)));
    }

    [Fact]
    public void Reconstitute_WithReleaseNotAfterReservation_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new StringIdentityReservation(
                UserIdentity.New(),
                new StringIdentity("account.name"),
                _reservedAt,
                _reservedAt));
    }

    [Fact]
    public void Activity_EndsExactlyAtFixedReleaseTime()
    {
        var reservation = StringIdentityReservation.Create(
            UserIdentity.New(),
            new StringIdentity("account.name"),
            _reservedAt,
            TimeSpan.FromDays(_reservationPeriodDays));

        Assert.True(reservation.IsActiveAt(_reservedAt));
        Assert.True(reservation.IsActiveAt(reservation.ReleasesAt.AddTicks(-1)));
        Assert.False(reservation.IsActiveAt(reservation.ReleasesAt));
    }
}

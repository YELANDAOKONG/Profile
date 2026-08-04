using Profile.Domain.Content.Pages;
using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Pages;

public sealed class PageRouteReservationTests
{
    private const int _reservationPeriodDays = 30;

    private static readonly DateTimeOffset _reservedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesScopeAndFixedReleaseTime()
    {
        var pageId = PageIdentity.New();
        var ownerId = UserIdentity.New();
        var routeIdentifier = new PageRouteIdentifier("About");
        var period = TimeSpan.FromDays(_reservationPeriodDays);

        var reservation = PageRouteReservation.Create(
            pageId,
            ownerId,
            routeIdentifier,
            _reservedAt,
            period);

        Assert.Equal(pageId, reservation.PageId);
        Assert.Equal(ownerId, reservation.OwnerId);
        Assert.Equal(routeIdentifier, reservation.RouteIdentifier);
        Assert.Equal(_reservedAt, reservation.ReservedAt);
        Assert.Equal(_reservedAt.Add(period), reservation.ReleasesAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePeriod_ThrowsArgumentOutOfRangeException(
        int periodDays)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PageRouteReservation.Create(
                PageIdentity.New(),
                UserIdentity.New(),
                new PageRouteIdentifier("about"),
                _reservedAt,
                TimeSpan.FromDays(periodDays)));
    }

    [Fact]
    public void Reconstitute_WithReleaseNotAfterReservation_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PageRouteReservation(
                PageIdentity.New(),
                UserIdentity.New(),
                new PageRouteIdentifier("about"),
                _reservedAt,
                _reservedAt));
    }

    [Fact]
    public void Activity_EndsExactlyAtFixedReleaseTime()
    {
        var reservation = PageRouteReservation.Create(
            PageIdentity.New(),
            UserIdentity.New(),
            new PageRouteIdentifier("about"),
            _reservedAt,
            TimeSpan.FromDays(_reservationPeriodDays));

        Assert.True(reservation.IsActiveAt(_reservedAt));
        Assert.True(reservation.IsActiveAt(reservation.ReleasesAt.AddTicks(-1)));
        Assert.False(reservation.IsActiveAt(reservation.ReleasesAt));
    }

    [Fact]
    public void DefaultReservationPeriod_IsNinetyDays()
    {
        Assert.Equal(
            TimeSpan.FromDays(PageRouteReservation.DefaultReservationPeriodDays),
            PageRouteReservation.DefaultReservationPeriod);
    }
}

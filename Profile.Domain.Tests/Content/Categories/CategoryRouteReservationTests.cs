using Profile.Domain.Content.Categories;
using Profile.Domain.Content.Categories.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Categories;

public sealed class CategoryRouteReservationTests
{
    private const int _reservationPeriodDays = 30;

    private static readonly DateTimeOffset _reservedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesScopeAndFixedReleaseTime()
    {
        var categoryId = CategoryIdentity.New();
        var ownerId = UserIdentity.New();
        var routeIdentifier = new TaxonomyRouteIdentifier("Engineering");
        var period = TimeSpan.FromDays(_reservationPeriodDays);

        var reservation = CategoryRouteReservation.Create(
            categoryId,
            ownerId,
            routeIdentifier,
            _reservedAt,
            period);

        Assert.Equal(categoryId, reservation.CategoryId);
        Assert.Equal(ownerId, reservation.OwnerId);
        Assert.Same(routeIdentifier, reservation.RouteIdentifier);
        Assert.Equal(_reservedAt, reservation.ReservedAt);
        Assert.Equal(_reservedAt.Add(period), reservation.ReleasesAt);
    }

    [Fact]
    public void Create_WithNonPositivePeriod_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CategoryRouteReservation.Create(
                CategoryIdentity.New(),
                UserIdentity.New(),
                new TaxonomyRouteIdentifier("engineering"),
                _reservedAt,
                TimeSpan.Zero));
    }

    [Fact]
    public void Reconstitute_WithReleaseNotAfterReservation_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CategoryRouteReservation(
                CategoryIdentity.New(),
                UserIdentity.New(),
                new TaxonomyRouteIdentifier("engineering"),
                _reservedAt,
                _reservedAt));
    }

    [Fact]
    public void Activity_EndsExactlyAtFixedReleaseTime()
    {
        var reservation = CategoryRouteReservation.Create(
            CategoryIdentity.New(),
            UserIdentity.New(),
            new TaxonomyRouteIdentifier("engineering"),
            _reservedAt,
            TimeSpan.FromDays(_reservationPeriodDays));

        Assert.True(reservation.IsActiveAt(_reservedAt));
        Assert.True(reservation.IsActiveAt(reservation.ReleasesAt.AddTicks(-1)));
        Assert.False(reservation.IsActiveAt(reservation.ReleasesAt));
    }
}

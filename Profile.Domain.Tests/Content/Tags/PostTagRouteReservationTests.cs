using Profile.Domain.Content.Tags;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Tags;

public sealed class PostTagRouteReservationTests
{
    private const int _reservationPeriodDays = 30;

    private static readonly DateTimeOffset _reservedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesScopeAndFixedReleaseTime()
    {
        var tagId = PostTagIdentity.New();
        var ownerId = UserIdentity.New();
        var routeIdentifier = new TaxonomyRouteIdentifier("DotNet");
        var period = TimeSpan.FromDays(_reservationPeriodDays);

        var reservation = PostTagRouteReservation.Create(
            tagId,
            ownerId,
            routeIdentifier,
            _reservedAt,
            period);

        Assert.Equal(tagId, reservation.TagId);
        Assert.Equal(ownerId, reservation.OwnerId);
        Assert.Same(routeIdentifier, reservation.RouteIdentifier);
        Assert.Equal(_reservedAt, reservation.ReservedAt);
        Assert.Equal(_reservedAt.Add(period), reservation.ReleasesAt);
    }

    [Fact]
    public void Create_WithNonPositivePeriod_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PostTagRouteReservation.Create(
                PostTagIdentity.New(),
                UserIdentity.New(),
                new TaxonomyRouteIdentifier("dotnet"),
                _reservedAt,
                TimeSpan.Zero));
    }

    [Fact]
    public void Reconstitute_WithReleaseNotAfterReservation_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PostTagRouteReservation(
                PostTagIdentity.New(),
                UserIdentity.New(),
                new TaxonomyRouteIdentifier("dotnet"),
                _reservedAt,
                _reservedAt));
    }

    [Fact]
    public void Activity_EndsExactlyAtFixedReleaseTime()
    {
        var reservation = PostTagRouteReservation.Create(
            PostTagIdentity.New(),
            UserIdentity.New(),
            new TaxonomyRouteIdentifier("dotnet"),
            _reservedAt,
            TimeSpan.FromDays(_reservationPeriodDays));

        Assert.True(reservation.IsActiveAt(_reservedAt));
        Assert.True(reservation.IsActiveAt(reservation.ReleasesAt.AddTicks(-1)));
        Assert.False(reservation.IsActiveAt(reservation.ReleasesAt));
    }
}

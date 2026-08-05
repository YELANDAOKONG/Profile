using Profile.Domain.Content.Categories.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Categories;

public sealed record CategoryRouteReservation
{
    public const int DefaultReservationPeriodDays = 90;

    public CategoryRouteReservation(
        CategoryIdentity categoryId,
        UserIdentity ownerId,
        TaxonomyRouteIdentifier routeIdentifier,
        DateTimeOffset reservedAt,
        DateTimeOffset releasesAt)
    {
        ArgumentNullException.ThrowIfNull(categoryId);
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(routeIdentifier);

        if (releasesAt <= reservedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(releasesAt),
                releasesAt,
                "Category route release time must be later than its reservation time.");
        }

        CategoryId = categoryId;
        OwnerId = ownerId;
        RouteIdentifier = routeIdentifier;
        ReservedAt = reservedAt;
        ReleasesAt = releasesAt;
    }

    public CategoryIdentity CategoryId { get; }

    public UserIdentity OwnerId { get; }

    public TaxonomyRouteIdentifier RouteIdentifier { get; }

    public DateTimeOffset ReservedAt { get; }

    public DateTimeOffset ReleasesAt { get; }

    public static TimeSpan DefaultReservationPeriod =>
        TimeSpan.FromDays(DefaultReservationPeriodDays);

    public static CategoryRouteReservation Create(
        CategoryIdentity categoryId,
        UserIdentity ownerId,
        TaxonomyRouteIdentifier routeIdentifier,
        DateTimeOffset reservedAt,
        TimeSpan reservationPeriod)
    {
        if (reservationPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reservationPeriod),
                reservationPeriod,
                "Category route reservation period must be greater than zero.");
        }

        return new CategoryRouteReservation(
            categoryId,
            ownerId,
            routeIdentifier,
            reservedAt,
            reservedAt.Add(reservationPeriod));
    }

    public bool IsActiveAt(DateTimeOffset timestamp) =>
        timestamp >= ReservedAt && timestamp < ReleasesAt;
}

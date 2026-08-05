using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Tags;

public sealed record BlogTagRouteReservation
{
    public const int DefaultReservationPeriodDays = 90;

    public BlogTagRouteReservation(
        BlogTagIdentity tagId,
        UserIdentity ownerId,
        TaxonomyRouteIdentifier routeIdentifier,
        DateTimeOffset reservedAt,
        DateTimeOffset releasesAt)
    {
        ArgumentNullException.ThrowIfNull(tagId);
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(routeIdentifier);

        if (releasesAt <= reservedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(releasesAt),
                releasesAt,
                "Blog tag route release time must be later than its reservation time.");
        }

        TagId = tagId;
        OwnerId = ownerId;
        RouteIdentifier = routeIdentifier;
        ReservedAt = reservedAt;
        ReleasesAt = releasesAt;
    }

    public BlogTagIdentity TagId { get; }

    public UserIdentity OwnerId { get; }

    public TaxonomyRouteIdentifier RouteIdentifier { get; }

    public DateTimeOffset ReservedAt { get; }

    public DateTimeOffset ReleasesAt { get; }

    public static TimeSpan DefaultReservationPeriod =>
        TimeSpan.FromDays(DefaultReservationPeriodDays);

    public static BlogTagRouteReservation Create(
        BlogTagIdentity tagId,
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
                "Blog tag route reservation period must be greater than zero.");
        }

        return new BlogTagRouteReservation(
            tagId,
            ownerId,
            routeIdentifier,
            reservedAt,
            reservedAt.Add(reservationPeriod));
    }

    public bool IsActiveAt(DateTimeOffset timestamp) =>
        timestamp >= ReservedAt && timestamp < ReleasesAt;
}

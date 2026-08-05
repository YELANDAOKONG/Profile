using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Tags;

public sealed record MomentTagRouteReservation
{
    public const int DefaultReservationPeriodDays = 90;

    public MomentTagRouteReservation(
        MomentTagIdentity tagId,
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
                "Moment tag route release time must be later than its reservation time.");
        }

        TagId = tagId;
        OwnerId = ownerId;
        RouteIdentifier = routeIdentifier;
        ReservedAt = reservedAt;
        ReleasesAt = releasesAt;
    }

    public MomentTagIdentity TagId { get; }

    public UserIdentity OwnerId { get; }

    public TaxonomyRouteIdentifier RouteIdentifier { get; }

    public DateTimeOffset ReservedAt { get; }

    public DateTimeOffset ReleasesAt { get; }

    public static TimeSpan DefaultReservationPeriod =>
        TimeSpan.FromDays(DefaultReservationPeriodDays);

    public static MomentTagRouteReservation Create(
        MomentTagIdentity tagId,
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
                "Moment tag route reservation period must be greater than zero.");
        }

        return new MomentTagRouteReservation(
            tagId,
            ownerId,
            routeIdentifier,
            reservedAt,
            reservedAt.Add(reservationPeriod));
    }

    public bool IsActiveAt(DateTimeOffset timestamp) =>
        timestamp >= ReservedAt && timestamp < ReleasesAt;
}

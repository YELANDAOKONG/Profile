using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Pages;

public sealed record PageRouteReservation
{
    public const int DefaultReservationPeriodDays = 90;

    public PageRouteReservation(
        PageIdentity pageId,
        UserIdentity ownerId,
        PageRouteIdentifier routeIdentifier,
        DateTimeOffset reservedAt,
        DateTimeOffset releasesAt)
    {
        ArgumentNullException.ThrowIfNull(pageId);
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(routeIdentifier);

        if (releasesAt <= reservedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(releasesAt),
                releasesAt,
                "Page route release time must be later than its reservation time.");
        }

        PageId = pageId;
        OwnerId = ownerId;
        RouteIdentifier = routeIdentifier;
        ReservedAt = reservedAt;
        ReleasesAt = releasesAt;
    }

    public PageIdentity PageId { get; }

    public UserIdentity OwnerId { get; }

    public PageRouteIdentifier RouteIdentifier { get; }

    public DateTimeOffset ReservedAt { get; }

    public DateTimeOffset ReleasesAt { get; }

    public static TimeSpan DefaultReservationPeriod =>
        TimeSpan.FromDays(DefaultReservationPeriodDays);

    public static PageRouteReservation Create(
        PageIdentity pageId,
        UserIdentity ownerId,
        PageRouteIdentifier routeIdentifier,
        DateTimeOffset reservedAt,
        TimeSpan reservationPeriod)
    {
        if (reservationPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reservationPeriod),
                reservationPeriod,
                "Page route reservation period must be greater than zero.");
        }

        return new PageRouteReservation(
            pageId,
            ownerId,
            routeIdentifier,
            reservedAt,
            reservedAt.Add(reservationPeriod));
    }

    public bool IsActiveAt(DateTimeOffset timestamp) =>
        timestamp >= ReservedAt && timestamp < ReleasesAt;
}

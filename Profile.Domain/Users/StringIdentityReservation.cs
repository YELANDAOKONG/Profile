using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed record StringIdentityReservation
{
    public const int DefaultReservationPeriodDays = 90;

    public StringIdentityReservation(
        UserIdentity previousOwnerId,
        StringIdentity stringId,
        DateTimeOffset reservedAt,
        DateTimeOffset releasesAt)
    {
        ArgumentNullException.ThrowIfNull(previousOwnerId);
        ArgumentNullException.ThrowIfNull(stringId);

        if (releasesAt <= reservedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(releasesAt),
                releasesAt,
                "String identity release time must be later than its reservation time.");
        }

        PreviousOwnerId = previousOwnerId;
        StringId = stringId;
        ReservedAt = reservedAt;
        ReleasesAt = releasesAt;
    }

    public UserIdentity PreviousOwnerId { get; }

    public StringIdentity StringId { get; }

    public DateTimeOffset ReservedAt { get; }

    public DateTimeOffset ReleasesAt { get; }

    public static TimeSpan DefaultReservationPeriod =>
        TimeSpan.FromDays(DefaultReservationPeriodDays);

    public static StringIdentityReservation Create(
        UserIdentity previousOwnerId,
        StringIdentity stringId,
        DateTimeOffset reservedAt,
        TimeSpan reservationPeriod)
    {
        if (reservationPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reservationPeriod),
                reservationPeriod,
                "String identity reservation period must be greater than zero.");
        }

        return new StringIdentityReservation(
            previousOwnerId,
            stringId,
            reservedAt,
            reservedAt.Add(reservationPeriod));
    }

    public bool IsActiveAt(DateTimeOffset timestamp) =>
        timestamp >= ReservedAt && timestamp < ReleasesAt;
}

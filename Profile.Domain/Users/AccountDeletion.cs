namespace Profile.Domain.Users;

public sealed record AccountDeletion
{
    public AccountDeletion(
        DateTimeOffset requestedAt,
        DateTimeOffset recoveryEndsAt)
    {
        if (recoveryEndsAt <= requestedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recoveryEndsAt),
                recoveryEndsAt,
                "Recovery end time must be later than the deletion request time.");
        }

        RequestedAt = requestedAt;
        RecoveryEndsAt = recoveryEndsAt;
    }

    public DateTimeOffset RequestedAt { get; }

    public DateTimeOffset RecoveryEndsAt { get; }
}

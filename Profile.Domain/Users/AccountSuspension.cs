namespace Profile.Domain.Users;

public sealed record AccountSuspension
{
    public AccountSuspension(
        DateTimeOffset suspendedAt,
        DateTimeOffset? expiresAt,
        string? reason = null)
    {
        if (expiresAt <= suspendedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAt),
                expiresAt,
                "Suspension expiration must be later than its start time.");
        }

        SuspendedAt = suspendedAt;
        ExpiresAt = expiresAt;
        Reason = reason;
    }

    public DateTimeOffset SuspendedAt { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public string? Reason { get; }
}

namespace Profile.Domain.Users;

public sealed record AccountBan
{
    public AccountBan(
        DateTimeOffset bannedAt,
        DateTimeOffset? expiresAt,
        string? reason = null)
    {
        if (expiresAt <= bannedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAt),
                expiresAt,
                "Ban expiration must be later than its start time.");
        }

        BannedAt = bannedAt;
        ExpiresAt = expiresAt;
        Reason = reason;
    }

    public DateTimeOffset BannedAt { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public string? Reason { get; }
}

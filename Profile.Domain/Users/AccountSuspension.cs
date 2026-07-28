namespace Profile.Domain.Users;

public sealed record AccountSuspension(
    DateTimeOffset SuspendedAt,
    DateTimeOffset? ExpiresAt);

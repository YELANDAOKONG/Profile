namespace Profile.Domain.Users;

public sealed record AccountBan(
    DateTimeOffset BannedAt,
    DateTimeOffset? ExpiresAt,
    string? Reason = null);

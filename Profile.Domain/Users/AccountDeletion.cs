namespace Profile.Domain.Users;

public sealed record AccountDeletion(
    DateTimeOffset RequestedAt,
    DateTimeOffset RecoveryEndsAt);

namespace Profile.Domain.Users.Value;

public sealed record AccountEmail(
    EmailAddress Address,
    DateTimeOffset? VerifiedAt);

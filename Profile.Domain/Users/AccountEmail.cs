using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed record AccountEmail(
    EmailAddress Address,
    DateTimeOffset? VerifiedAt);

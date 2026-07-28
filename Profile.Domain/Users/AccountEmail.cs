using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed record AccountEmail
{
    public AccountEmail(
        EmailAddress address,
        DateTimeOffset? verifiedAt)
    {
        ArgumentNullException.ThrowIfNull(address);

        Address = address;
        VerifiedAt = verifiedAt;
    }

    public EmailAddress Address { get; }

    public DateTimeOffset? VerifiedAt { get; }
}

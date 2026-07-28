namespace Profile.Domain.Users.Value;

public sealed record StringIdentity(string Value, string NormalizedValue)
{
    public const int MinimumLength = 5;
    public const int MaximumLength = 64;
}

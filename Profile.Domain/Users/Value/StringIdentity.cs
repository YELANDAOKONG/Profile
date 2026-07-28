namespace Profile.Domain.Users.Value;

public sealed record StringIdentity
{
    public const int MinimumLength = 5;
    public const int MaximumLength = 64;

    public StringIdentity(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length is < MinimumLength or > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"String identity length must be between {MinimumLength} and {MaximumLength} characters.");
        }

        if (!value.All(IsAllowedCharacter))
        {
            throw new ArgumentException(
                "String identity contains a disallowed character.",
                nameof(value));
        }

        if (value.StartsWith('.') || value.EndsWith('.'))
        {
            throw new ArgumentException(
                "String identity cannot start or end with a period.",
                nameof(value));
        }

        if (value.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "String identity cannot contain consecutive periods.",
                nameof(value));
        }

        Value = value;
        NormalizedValue = value.ToUpperInvariant();
    }

    public string Value { get; }

    public string NormalizedValue { get; }

    public bool Equals(StringIdentity? other) =>
        other is not null &&
        string.Equals(NormalizedValue, other.NormalizedValue, StringComparison.Ordinal);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(NormalizedValue);

    private static bool IsAllowedCharacter(char character) =>
        character is >= 'A' and <= 'Z' or
            >= 'a' and <= 'z' or
            >= '0' and <= '9' or
            '_' or
            '.';
}

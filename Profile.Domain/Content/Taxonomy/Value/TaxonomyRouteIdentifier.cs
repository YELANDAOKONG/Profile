namespace Profile.Domain.Content.Taxonomy.Value;

public sealed record TaxonomyRouteIdentifier
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 128;

    public TaxonomyRouteIdentifier(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length is < MinimumLength or > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Taxonomy route identifier length must be between {MinimumLength} and {MaximumLength} characters.");
        }

        if (!value.All(IsAllowedCharacter))
        {
            throw new ArgumentException(
                "Taxonomy route identifier contains a disallowed character.",
                nameof(value));
        }

        if (!IsAsciiLetterOrDigit(value[0]) ||
            !IsAsciiLetterOrDigit(value[^1]))
        {
            throw new ArgumentException(
                "Taxonomy route identifier must start and end with an ASCII letter or digit.",
                nameof(value));
        }

        if (value.Contains("--", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Taxonomy route identifier cannot contain consecutive hyphens.",
                nameof(value));
        }

        Value = value;
        NormalizedValue = value.ToLowerInvariant();
    }

    public string Value { get; }

    public string NormalizedValue { get; }

    public bool Equals(TaxonomyRouteIdentifier? other) =>
        other is not null &&
        string.Equals(
            NormalizedValue,
            other.NormalizedValue,
            StringComparison.Ordinal);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(NormalizedValue);

    private static bool IsAllowedCharacter(char character) =>
        IsAsciiLetterOrDigit(character) || character is '-';

    private static bool IsAsciiLetterOrDigit(char character) =>
        character is >= 'A' and <= 'Z' or
            >= 'a' and <= 'z' or
            >= '0' and <= '9';
}

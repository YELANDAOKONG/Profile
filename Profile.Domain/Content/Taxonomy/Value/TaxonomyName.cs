namespace Profile.Domain.Content.Taxonomy.Value;

public sealed record TaxonomyName
{
    public const int MaximumLength = 64;

    public TaxonomyName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Taxonomy name cannot be empty or whitespace.",
                nameof(value));
        }

        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            throw new ArgumentException(
                "Taxonomy name cannot contain surrounding whitespace.",
                nameof(value));
        }

        if (value.Length > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Taxonomy name length cannot exceed {MaximumLength} characters.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

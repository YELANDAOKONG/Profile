namespace Profile.Domain.Users.Value;

public sealed record FontFamily
{
    public const int MaximumLength = 128;

    public FontFamily(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Font family cannot be empty or whitespace.",
                nameof(value));
        }

        if (value.Length > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Font family length cannot exceed {MaximumLength} characters.");
        }

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Font family cannot contain surrounding whitespace.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}

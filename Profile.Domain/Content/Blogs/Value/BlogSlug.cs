namespace Profile.Domain.Content.Blogs.Value;

public sealed record BlogSlug
{
    public const int MinimumLength = 9;

    public BlogSlug(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length < MinimumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Blog slug length cannot be less than {MinimumLength} digits.");
        }

        if (!value.All(IsAsciiDigit))
        {
            throw new ArgumentException(
                "Blog slug can contain only ASCII digits.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    private static bool IsAsciiDigit(char character) =>
        character is >= '0' and <= '9';
}

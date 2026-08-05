namespace Profile.Domain.Content.Collections.Value;

public sealed record CollectionFolderName
{
    public const int MaximumLength = 128;

    public CollectionFolderName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Collection folder name cannot be empty or whitespace.",
                nameof(value));
        }

        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            throw new ArgumentException(
                "Collection folder name cannot contain surrounding whitespace.",
                nameof(value));
        }

        if (value.Length > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Collection folder name length cannot exceed {MaximumLength} characters.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

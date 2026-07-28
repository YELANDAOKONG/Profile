namespace Profile.Domain.Content.Blocks;

public sealed record CodeBlock : ContentBlock
{
    public CodeBlock(string source, string language)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(language);

        EnsureTextWithinLimit(source, nameof(source));

        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException(
                "Code block language cannot be empty or whitespace.",
                nameof(language));
        }

        if (char.IsWhiteSpace(language[0]) || char.IsWhiteSpace(language[^1]))
        {
            throw new ArgumentException(
                "Code block language cannot contain surrounding whitespace.",
                nameof(language));
        }

        Source = source;
        Language = language;
    }

    // The language identifier keeps its original casing because common
    // identifiers are not uniformly lowercase ("C#", "F#").
    public string Source { get; }

    public string Language { get; }
}

namespace Profile.Domain.Content.Value;

public sealed record ContentBody
{
    public ContentBody(string source, ContentFormat format)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!Enum.IsDefined(format))
        {
            throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                "Content format is not supported.");
        }

        Source = source;
        Format = format;
    }

    // Empty source is allowed here because each aggregate applies its own
    // emptiness and length rules (draft autosave, media-only posts, etc.).
    public string Source { get; }

    public ContentFormat Format { get; }
}

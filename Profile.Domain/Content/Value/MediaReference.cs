using Profile.Domain.Media.Value;

namespace Profile.Domain.Content.Value;

public sealed record MediaReference
{
    public MediaReference(MediaItemIdentity mediaId, string? altText = null)
    {
        ArgumentNullException.ThrowIfNull(mediaId);

        if (altText is not null && string.IsNullOrWhiteSpace(altText))
        {
            throw new ArgumentException(
                "Media alt text cannot be empty or whitespace.",
                nameof(altText));
        }

        if (altText is not null &&
            (char.IsWhiteSpace(altText[0]) || char.IsWhiteSpace(altText[^1])))
        {
            throw new ArgumentException(
                "Media alt text cannot contain surrounding whitespace.",
                nameof(altText));
        }

        MediaId = mediaId;
        AltText = altText;
    }

    public MediaItemIdentity MediaId { get; }

    // Alt text lives on the reference rather than the media item because the
    // same item can need different descriptions in different content.
    public string? AltText { get; }
}

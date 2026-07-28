using Profile.Domain.Content.Value;

namespace Profile.Domain.Content.Blocks;

public sealed record MediaBlock : ContentBlock
{
    public MediaBlock(MediaReference media)
    {
        ArgumentNullException.ThrowIfNull(media);

        Media = media;
    }

    public MediaReference Media { get; }
}

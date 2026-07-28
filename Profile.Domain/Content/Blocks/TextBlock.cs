using Profile.Domain.Content.Value;

namespace Profile.Domain.Content.Blocks;

public sealed record TextBlock : ContentBlock
{
    public TextBlock(ContentBody body)
    {
        ArgumentNullException.ThrowIfNull(body);

        EnsureTextWithinLimit(body.Source, nameof(body));

        Body = body;
    }

    public ContentBody Body { get; }
}

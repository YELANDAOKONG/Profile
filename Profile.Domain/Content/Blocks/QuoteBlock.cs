using Profile.Domain.Content.Value;

namespace Profile.Domain.Content.Blocks;

// A blockquote carries quoted text only. It is deliberately unrelated to the
// Post/Moment quote relationships, which reference other content aggregates.
public sealed record QuoteBlock : ContentBlock
{
    public QuoteBlock(ContentBody body)
    {
        ArgumentNullException.ThrowIfNull(body);

        EnsureTextWithinLimit(body.Source, nameof(body));

        Body = body;
    }

    public ContentBody Body { get; }
}

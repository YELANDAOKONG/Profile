using System.Collections.ObjectModel;

namespace Profile.Domain.Content.Blocks;

public sealed record ContentBlockCollection
{
    public const int MaximumCount = 8_192;

    private readonly ReadOnlyCollection<ContentBlock> _blocks;

    public ContentBlockCollection(IEnumerable<ContentBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var items = blocks.ToArray();

        if (items.Length > MaximumCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(blocks),
                items.Length,
                $"Content block count cannot exceed {MaximumCount}.");
        }

        if (items.Any(static block => block is null))
        {
            throw new ArgumentException(
                "Content blocks cannot contain a null item.",
                nameof(blocks));
        }

        _blocks = Array.AsReadOnly(items);
    }

    public IReadOnlyList<ContentBlock> Blocks => _blocks;

    public bool Equals(ContentBlockCollection? other) =>
        other is not null && _blocks.SequenceEqual(other._blocks);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var block in _blocks)
        {
            hashCode.Add(block);
        }

        return hashCode.ToHashCode();
    }
}

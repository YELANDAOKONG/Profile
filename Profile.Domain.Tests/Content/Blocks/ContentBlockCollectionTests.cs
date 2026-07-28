using Profile.Domain.Content.Blocks;

namespace Profile.Domain.Tests.Content.Blocks;

public sealed class ContentBlockCollectionTests
{
    [Fact]
    public void Constructor_WithEmptyCollection_AllowsValue()
    {
        var collection = new ContentBlockCollection([]);

        Assert.Empty(collection.Blocks);
    }

    [Fact]
    public void Constructor_WithOrderedBlocks_PreservesOrder()
    {
        ContentBlock[] blocks = [new DividerBlock(), new SpacerBlock()];

        var collection = new ContentBlockCollection(blocks);

        Assert.IsType<DividerBlock>(collection.Blocks[0]);
        Assert.IsType<SpacerBlock>(collection.Blocks[1]);
    }

    [Fact]
    public void Constructor_WithMaximumCount_AcceptsValue()
    {
        var blocks = Enumerable.Repeat<ContentBlock>(
            new SpacerBlock(),
            ContentBlockCollection.MaximumCount);

        var collection = new ContentBlockCollection(blocks);

        Assert.Equal(ContentBlockCollection.MaximumCount, collection.Blocks.Count);
    }

    [Fact]
    public void Constructor_WithCountAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        var blocks = Enumerable.Repeat<ContentBlock>(
            new SpacerBlock(),
            ContentBlockCollection.MaximumCount + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ContentBlockCollection(blocks));

        Assert.Equal("blocks", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCollection_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ContentBlockCollection(null!));

        Assert.Equal("blocks", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullItem_ThrowsArgumentException()
    {
        ContentBlock[] blocks = [new SpacerBlock(), null!];

        var exception = Assert.Throws<ArgumentException>(
            () => new ContentBlockCollection(blocks));

        Assert.Equal("blocks", exception.ParamName);
    }

    [Fact]
    public void Constructor_CopiesSourceCollection()
    {
        List<ContentBlock> blocks = [new DividerBlock()];
        var collection = new ContentBlockCollection(blocks);

        blocks.Add(new SpacerBlock());

        Assert.Single(collection.Blocks);
        Assert.IsType<DividerBlock>(collection.Blocks[0]);
    }

    [Fact]
    public void Equality_WithEqualOrderedBlocks_TreatsCollectionsAsEqual()
    {
        var first = new ContentBlockCollection([new DividerBlock(), new SpacerBlock()]);
        var second = new ContentBlockCollection([new DividerBlock(), new SpacerBlock()]);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentOrder_TreatsCollectionsAsDistinct()
    {
        var first = new ContentBlockCollection([new DividerBlock(), new SpacerBlock()]);
        var second = new ContentBlockCollection([new SpacerBlock(), new DividerBlock()]);

        Assert.NotEqual(first, second);
    }
}

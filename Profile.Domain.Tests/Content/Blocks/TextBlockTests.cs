using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Blocks;

public sealed class TextBlockTests
{
    [Fact]
    public void Constructor_WithValidBody_PreservesBody()
    {
        var body = new ContentBody("Hello", ContentFormat.PlainText);

        var block = new TextBlock(body);

        Assert.Equal(body, block.Body);
    }

    [Fact]
    public void Constructor_WithMaximumLengthBody_AcceptsValue()
    {
        var body = new ContentBody(
            new string('a', ContentBlock.MaximumTextLength),
            ContentFormat.PlainText);

        var block = new TextBlock(body);

        Assert.Equal(ContentBlock.MaximumTextLength, block.Body.Source.Length);
    }

    [Fact]
    public void Constructor_WithBodyAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var body = new ContentBody(
            new string('a', ContentBlock.MaximumTextLength + 1),
            ContentFormat.PlainText);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new TextBlock(body));

        Assert.Equal("body", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new TextBlock(null!));

        Assert.Equal("body", exception.ParamName);
    }
}

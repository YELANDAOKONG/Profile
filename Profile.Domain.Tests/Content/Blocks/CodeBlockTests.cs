using Profile.Domain.Content.Blocks;

namespace Profile.Domain.Tests.Content.Blocks;

public sealed class CodeBlockTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesSourceAndLanguage()
    {
        var block = new CodeBlock("Console.WriteLine();", "C#");

        Assert.Equal("Console.WriteLine();", block.Source);
        Assert.Equal("C#", block.Language);
    }

    [Fact]
    public void Constructor_WithMaximumLengthSource_AcceptsValue()
    {
        var source = new string('a', ContentBlock.MaximumTextLength);

        var block = new CodeBlock(source, "text");

        Assert.Equal(ContentBlock.MaximumTextLength, block.Source.Length);
    }

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CodeBlock(null!, "csharp"));

        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithSourceAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var source = new string('a', ContentBlock.MaximumTextLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CodeBlock(source, "csharp"));

        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLanguage_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CodeBlock("code", null!));

        Assert.Equal("language", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceLanguage_ThrowsArgumentException(
        string language)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CodeBlock("code", language));

        Assert.Equal("language", exception.ParamName);
    }

    [Theory]
    [InlineData(" csharp")]
    [InlineData("csharp ")]
    public void Constructor_WithSurroundingWhitespaceLanguage_ThrowsArgumentException(
        string language)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CodeBlock("code", language));

        Assert.Equal("language", exception.ParamName);
    }
}

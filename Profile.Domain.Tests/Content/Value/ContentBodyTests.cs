using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Value;

public sealed class ContentBodyTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesSourceAndFormat()
    {
        var body = new ContentBody("# Hello", ContentFormat.Markdown);

        Assert.Equal("# Hello", body.Source);
        Assert.Equal(ContentFormat.Markdown, body.Format);
    }

    [Fact]
    public void Constructor_WithEmptySource_AllowsValue()
    {
        var body = new ContentBody(string.Empty, ContentFormat.PlainText);

        Assert.Equal(string.Empty, body.Source);
    }

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ContentBody(null!, ContentFormat.PlainText));

        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUndefinedFormat_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ContentBody("text", (ContentFormat)999));

        Assert.Equal("format", exception.ParamName);
    }

    [Fact]
    public void Equality_WithSameSourceAndFormat_TreatsValuesAsEqual()
    {
        var first = new ContentBody("text", ContentFormat.PlainText);
        var second = new ContentBody("text", ContentFormat.PlainText);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentFormat_TreatsValuesAsDistinct()
    {
        var first = new ContentBody("text", ContentFormat.PlainText);
        var second = new ContentBody("text", ContentFormat.Markdown);

        Assert.NotEqual(first, second);
    }
}

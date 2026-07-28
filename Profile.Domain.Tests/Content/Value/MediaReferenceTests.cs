using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;

namespace Profile.Domain.Tests.Content.Value;

public sealed class MediaReferenceTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesMediaAndAltText()
    {
        var mediaId = MediaItemIdentity.New();

        var reference = new MediaReference(mediaId, "A description");

        Assert.Equal(mediaId, reference.MediaId);
        Assert.Equal("A description", reference.AltText);
    }

    [Fact]
    public void Constructor_WithoutAltText_AllowsNull()
    {
        var reference = new MediaReference(MediaItemIdentity.New());

        Assert.Null(reference.AltText);
    }

    [Fact]
    public void Constructor_WithNullMediaId_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new MediaReference(null!));

        Assert.Equal("mediaId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceAltText_ThrowsArgumentException(string altText)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MediaReference(MediaItemIdentity.New(), altText));

        Assert.Equal("altText", exception.ParamName);
    }

    [Theory]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    public void Constructor_WithSurroundingWhitespaceAltText_ThrowsArgumentException(string altText)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MediaReference(MediaItemIdentity.New(), altText));

        Assert.Equal("altText", exception.ParamName);
    }
}

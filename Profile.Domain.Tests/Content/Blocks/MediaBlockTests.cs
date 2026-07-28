using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;

namespace Profile.Domain.Tests.Content.Blocks;

public sealed class MediaBlockTests
{
    [Fact]
    public void Constructor_WithValidMedia_PreservesMedia()
    {
        var media = new MediaReference(MediaItemIdentity.New(), "A photo");

        var block = new MediaBlock(media);

        Assert.Equal(media, block.Media);
    }

    [Fact]
    public void Constructor_WithNullMedia_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new MediaBlock(null!));

        Assert.Equal("media", exception.ParamName);
    }
}

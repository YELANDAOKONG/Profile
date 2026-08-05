using Profile.Domain.Content.Bookmarks.Value;

namespace Profile.Domain.Tests.Content.Bookmarks.Value;

public sealed class PostBookmarkFolderIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        Assert.Equal(value, new PostBookmarkFolderIdentity(value).Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new PostBookmarkFolderIdentity(Guid.Empty));
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        Assert.NotEqual(Guid.Empty, PostBookmarkFolderIdentity.New().Value);
    }
}

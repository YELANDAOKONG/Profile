using Profile.Domain.Content.Favorites.Value;

namespace Profile.Domain.Tests.Content.Favorites.Value;

public sealed class BlogFavoriteFolderIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        Assert.Equal(value, new BlogFavoriteFolderIdentity(value).Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new BlogFavoriteFolderIdentity(Guid.Empty));
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        Assert.NotEqual(Guid.Empty, BlogFavoriteFolderIdentity.New().Value);
    }
}

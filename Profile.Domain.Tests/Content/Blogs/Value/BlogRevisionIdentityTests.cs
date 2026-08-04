using Profile.Domain.Content.Blogs.Value;

namespace Profile.Domain.Tests.Content.Blogs.Value;

public sealed class BlogRevisionIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new BlogRevisionIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new BlogRevisionIdentity(Guid.Empty));
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        var identity = BlogRevisionIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Equality_WithSameGuid_TreatsValuesAsSameIdentity()
    {
        var value = Guid.NewGuid();

        var first = new BlogRevisionIdentity(value);
        var second = new BlogRevisionIdentity(value);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

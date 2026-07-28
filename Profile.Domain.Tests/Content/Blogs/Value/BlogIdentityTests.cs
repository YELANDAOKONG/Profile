using Profile.Domain.Content.Blogs.Value;

namespace Profile.Domain.Tests.Content.Blogs.Value;

public sealed class BlogIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new BlogIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new BlogIdentity(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        var identity = BlogIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Equality_WithSameGuid_TreatsValuesAsSameIdentity()
    {
        var value = Guid.NewGuid();
        var first = new BlogIdentity(value);
        var second = new BlogIdentity(value);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

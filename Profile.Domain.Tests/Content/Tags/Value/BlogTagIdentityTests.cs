using Profile.Domain.Content.Tags.Value;

namespace Profile.Domain.Tests.Content.Tags.Value;

public sealed class BlogTagIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new BlogTagIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new BlogTagIdentity(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        var identity = BlogTagIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Equality_WithSameGuid_TreatsValuesAsSameIdentity()
    {
        var value = Guid.NewGuid();
        var first = new BlogTagIdentity(value);
        var second = new BlogTagIdentity(value);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

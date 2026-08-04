using Profile.Domain.Content.Categories.Value;

namespace Profile.Domain.Tests.Content.Categories.Value;

public sealed class CategoryIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new CategoryIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CategoryIdentity(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        var identity = CategoryIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Equality_WithSameGuid_TreatsValuesAsSameIdentity()
    {
        var value = Guid.NewGuid();
        var first = new CategoryIdentity(value);
        var second = new CategoryIdentity(value);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

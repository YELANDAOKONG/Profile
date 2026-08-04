using Profile.Domain.Content.Tags.Value;

namespace Profile.Domain.Tests.Content.Tags.Value;

public sealed class MomentTagIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new MomentTagIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MomentTagIdentity(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        var identity = MomentTagIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Equality_WithSameGuid_TreatsValuesAsSameIdentity()
    {
        var value = Guid.NewGuid();

        Assert.Equal(new MomentTagIdentity(value), new MomentTagIdentity(value));
    }
}

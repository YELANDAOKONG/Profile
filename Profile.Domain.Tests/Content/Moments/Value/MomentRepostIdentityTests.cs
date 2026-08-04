using Profile.Domain.Content.Moments.Value;

namespace Profile.Domain.Tests.Content.Moments.Value;

public sealed class MomentRepostIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new MomentRepostIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MomentRepostIdentity(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        var identity = MomentRepostIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Equality_WithSameGuid_TreatsValuesAsSameIdentity()
    {
        var value = Guid.NewGuid();

        Assert.Equal(
            new MomentRepostIdentity(value),
            new MomentRepostIdentity(value));
    }
}

using Profile.Domain.Social.Value;

namespace Profile.Domain.Tests.Social.Value;

public sealed class FollowRequestIdentityTests
{
    [Fact]
    public void Constructor_WithGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        var identity = new FollowRequestIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithSameGuid_CreatesEqualIdentities()
    {
        var value = Guid.NewGuid();

        var first = new FollowRequestIdentity(value);
        var second = new FollowRequestIdentity(value);

        Assert.Equal(first, second);
    }

    [Fact]
    public void New_CreatesNonEmptyIdentity()
    {
        var identity = FollowRequestIdentity.New();

        Assert.NotEqual(Guid.Empty, identity.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new FollowRequestIdentity(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}

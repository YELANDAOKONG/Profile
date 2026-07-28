using Profile.Domain.Content.Blocks;

namespace Profile.Domain.Tests.Content.Blocks;

public sealed class DividerBlockTests
{
    [Fact]
    public void Equality_BetweenInstances_TreatsThemAsEqual()
    {
        var first = new DividerBlock();
        var second = new DividerBlock();

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

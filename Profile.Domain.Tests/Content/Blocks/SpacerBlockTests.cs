using Profile.Domain.Content.Blocks;

namespace Profile.Domain.Tests.Content.Blocks;

public sealed class SpacerBlockTests
{
    [Fact]
    public void Equality_BetweenInstances_TreatsThemAsEqual()
    {
        var first = new SpacerBlock();
        var second = new SpacerBlock();

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

using Profile.Domain.Content.Comments.Value;

namespace Profile.Domain.Tests.Content.Comments.Value;

public sealed class BlogCommentIdentityTests
{
    [Fact]
    public void Constructor_WithValidGuid_PreservesValue()
    {
        var value = Guid.NewGuid();

        Assert.Equal(value, new BlogCommentIdentity(value).Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new BlogCommentIdentity(Guid.Empty));
    }

    [Fact]
    public void New_ReturnsNonEmptyIdentity()
    {
        Assert.NotEqual(Guid.Empty, BlogCommentIdentity.New().Value);
    }
}

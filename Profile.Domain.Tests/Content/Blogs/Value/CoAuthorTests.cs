using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Blogs.Value;

public sealed class CoAuthorTests
{
    [Fact]
    public void FromUser_WithValidIdentity_PreservesUserReference()
    {
        var userId = UserIdentity.New();

        var coAuthor = CoAuthor.FromUser(userId);

        Assert.Equal(userId, coAuthor.UserId);
        Assert.Null(coAuthor.Text);
    }

    [Fact]
    public void FromUser_WithNullIdentity_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CoAuthor.FromUser(null!));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void FromText_WithValidText_PreservesText()
    {
        var coAuthor = CoAuthor.FromText("Guest Author");

        Assert.Null(coAuthor.UserId);
        Assert.Equal("Guest Author", coAuthor.Text);
    }

    [Fact]
    public void FromText_WithNullText_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CoAuthor.FromText(null!));

        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void FromText_AtMaximumLength_PreservesText()
    {
        var text = new string('a', CoAuthor.MaximumTextLength);

        var coAuthor = CoAuthor.FromText(text);

        Assert.Equal(text, coAuthor.Text);
    }

    [Fact]
    public void FromText_AboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var text = new string('a', CoAuthor.MaximumTextLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CoAuthor.FromText(text));

        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void Equality_WithSameUserReference_TreatsValuesAsEqual()
    {
        var userId = UserIdentity.New();

        var first = CoAuthor.FromUser(userId);
        var second = CoAuthor.FromUser(userId);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_WithSameText_TreatsValuesAsEqual()
    {
        var first = CoAuthor.FromText("Guest Author");
        var second = CoAuthor.FromText("Guest Author");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

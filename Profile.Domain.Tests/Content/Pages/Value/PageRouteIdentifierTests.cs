using Profile.Domain.Content.Pages.Value;

namespace Profile.Domain.Tests.Content.Pages.Value;

public sealed class PageRouteIdentifierTests
{
    [Fact]
    public void Constructor_WithUppercaseValue_PreservesValueAndNormalizesToLowercase()
    {
        var identifier = new PageRouteIdentifier("About-Us");

        Assert.Equal("About-Us", identifier.Value);
        Assert.Equal("about-us", identifier.NormalizedValue);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("A1")]
    [InlineData("about-us")]
    [InlineData("123")]
    public void Constructor_WithValidValue_AcceptsValue(string value)
    {
        var identifier = new PageRouteIdentifier(value);

        Assert.Equal(value, identifier.Value);
    }

    [Fact]
    public void Constructor_AtMaximumLength_AcceptsValue()
    {
        var value = new string('a', PageRouteIdentifier.MaximumLength);

        var identifier = new PageRouteIdentifier(value);

        Assert.Equal(value, identifier.Value);
    }

    [Fact]
    public void Constructor_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PageRouteIdentifier(null!));
    }

    [Fact]
    public void Constructor_WithEmptyValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PageRouteIdentifier(string.Empty));
    }

    [Fact]
    public void Constructor_AboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string(
            'a',
            PageRouteIdentifier.MaximumLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PageRouteIdentifier(value));
    }

    [Theory]
    [InlineData("about_us")]
    [InlineData("about.us")]
    [InlineData("關於")]
    public void Constructor_WithDisallowedCharacter_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(
            () => new PageRouteIdentifier(value));
    }

    [Theory]
    [InlineData("-about")]
    [InlineData("about-")]
    public void Constructor_WithHyphenAtBoundary_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(
            () => new PageRouteIdentifier(value));
    }

    [Fact]
    public void Constructor_WithConsecutiveHyphens_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new PageRouteIdentifier("about--us"));
    }

    [Fact]
    public void Equality_WithCasingDifference_TreatsValuesAsEqual()
    {
        var first = new PageRouteIdentifier("About-Us");
        var second = new PageRouteIdentifier("about-us");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

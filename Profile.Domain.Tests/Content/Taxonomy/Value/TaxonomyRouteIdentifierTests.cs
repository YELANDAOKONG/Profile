using Profile.Domain.Content.Taxonomy.Value;

namespace Profile.Domain.Tests.Content.Taxonomy.Value;

public sealed class TaxonomyRouteIdentifierTests
{
    [Fact]
    public void Constructor_WithUppercaseValue_PreservesValueAndNormalizesToLowercase()
    {
        var identifier = new TaxonomyRouteIdentifier("Web-Development");

        Assert.Equal("Web-Development", identifier.Value);
        Assert.Equal("web-development", identifier.NormalizedValue);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("A1")]
    [InlineData("web-development")]
    [InlineData("123")]
    public void Constructor_WithValidValue_AcceptsValue(string value)
    {
        var identifier = new TaxonomyRouteIdentifier(value);

        Assert.Equal(value, identifier.Value);
    }

    [Fact]
    public void Constructor_AtMaximumLength_AcceptsValue()
    {
        var value = new string('a', TaxonomyRouteIdentifier.MaximumLength);

        var identifier = new TaxonomyRouteIdentifier(value);

        Assert.Equal(value, identifier.Value);
    }

    [Fact]
    public void Constructor_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new TaxonomyRouteIdentifier(null!));
    }

    [Fact]
    public void Constructor_WithEmptyValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TaxonomyRouteIdentifier(string.Empty));
    }

    [Fact]
    public void Constructor_AboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string(
            'a',
            TaxonomyRouteIdentifier.MaximumLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TaxonomyRouteIdentifier(value));
    }

    [Theory]
    [InlineData("web_development")]
    [InlineData("web.development")]
    [InlineData("開發")]
    public void Constructor_WithDisallowedCharacter_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(
            () => new TaxonomyRouteIdentifier(value));
    }

    [Theory]
    [InlineData("-web")]
    [InlineData("web-")]
    public void Constructor_WithHyphenAtBoundary_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(
            () => new TaxonomyRouteIdentifier(value));
    }

    [Fact]
    public void Constructor_WithConsecutiveHyphens_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new TaxonomyRouteIdentifier("web--development"));
    }

    [Fact]
    public void Equality_WithCasingDifference_TreatsValuesAsEqual()
    {
        var first = new TaxonomyRouteIdentifier("Web-Development");
        var second = new TaxonomyRouteIdentifier("web-development");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

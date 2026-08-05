using Profile.Domain.Content.Taxonomy.Value;

namespace Profile.Domain.Tests.Content.Taxonomy.Value;

public sealed class TaxonomyNameTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesValue()
    {
        var name = new TaxonomyName("Engineering");

        Assert.Equal("Engineering", name.Value);
        Assert.Equal("Engineering", name.ToString());
    }

    [Fact]
    public void Constructor_AtMaximumLength_AcceptsValue()
    {
        var value = new string('a', TaxonomyName.MaximumLength);

        var name = new TaxonomyName(value);

        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void Constructor_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new TaxonomyName(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceValue_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(
            () => new TaxonomyName(value));
    }

    [Theory]
    [InlineData(" Engineering")]
    [InlineData("Engineering ")]
    [InlineData("\tEngineering")]
    public void Constructor_WithSurroundingWhitespace_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(
            () => new TaxonomyName(value));
    }

    [Fact]
    public void Constructor_AboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string('a', TaxonomyName.MaximumLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TaxonomyName(value));
    }

    [Fact]
    public void Equality_WithCasingDifference_TreatsValuesAsDifferent()
    {
        var first = new TaxonomyName("Engineering");
        var second = new TaxonomyName("engineering");

        Assert.NotEqual(first, second);
    }
}

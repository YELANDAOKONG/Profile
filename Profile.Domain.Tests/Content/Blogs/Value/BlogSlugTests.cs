using Profile.Domain.Content.Blogs.Value;

namespace Profile.Domain.Tests.Content.Blogs.Value;

public sealed class BlogSlugTests
{
    [Fact]
    public void Constructor_WithMinimumWidth_PreservesLeadingZeros()
    {
        const string value = "000000001";

        var slug = new BlogSlug(value);

        Assert.Equal(value, slug.Value);
    }

    [Fact]
    public void Constructor_WithWidthAboveMinimum_AcceptsValue()
    {
        const string value = "1000000000";

        var slug = new BlogSlug(value);

        Assert.Equal(value, slug.Value);
    }

    [Fact]
    public void Constructor_WithNullValue_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new BlogSlug(null!));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWidthBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        var value = new string('0', BlogSlug.MinimumLength - 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new BlogSlug(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("00000000a")]
    [InlineData("00000000.")]
    [InlineData("００００００００１")]
    [InlineData("١٢٣٤٥٦٧٨٩")]
    public void Constructor_WithNonAsciiDigit_ThrowsArgumentException(string value)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new BlogSlug(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Equality_WithSameValue_TreatsSlugsAsEqual()
    {
        var first = new BlogSlug("000000001");
        var second = new BlogSlug("000000001");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

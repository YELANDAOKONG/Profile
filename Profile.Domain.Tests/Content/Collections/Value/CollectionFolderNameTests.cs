using Profile.Domain.Content.Collections.Value;

namespace Profile.Domain.Tests.Content.Collections.Value;

public sealed class CollectionFolderNameTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesOriginalCasing()
    {
        var name = new CollectionFolderName("Read Later");

        Assert.Equal("Read Later", name.Value);
        Assert.Equal("Read Later", name.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" Read Later")]
    [InlineData("Read Later ")]
    public void Constructor_WithInvalidValue_ThrowsArgumentException(
        string value)
    {
        Assert.Throws<ArgumentException>(() => new CollectionFolderName(value));
    }

    [Fact]
    public void Constructor_WithMaximumLength_AllowsValue()
    {
        var value = new string('a', CollectionFolderName.MaximumLength);

        Assert.Equal(value, new CollectionFolderName(value).Value);
    }

    [Fact]
    public void Constructor_WithValueOverMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string('a', CollectionFolderName.MaximumLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CollectionFolderName(value));
    }

    [Fact]
    public void Equality_IsCaseSensitive()
    {
        Assert.NotEqual(
            new CollectionFolderName("Reading"),
            new CollectionFolderName("reading"));
    }
}

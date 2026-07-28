using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users.Value;

public sealed class EmailAddressTests
{
    [Fact]
    public void Constructor_WithValidValue_NormalizesWithoutChangingOriginalValue()
    {
        const string value = "User.Name+tag@example.com";

        var address = new EmailAddress(value);

        Assert.Equal(value, address.Value);
        Assert.Equal("USER.NAME+TAG@EXAMPLE.COM", address.NormalizedValue);
    }

    [Fact]
    public void Equality_WithDifferentCasing_TreatsValuesAsSameAddress()
    {
        var first = new EmailAddress("User@example.com");
        var second = new EmailAddress("user@EXAMPLE.COM");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Constructor_WithoutFullSyntaxValidation_AcceptsNonEmptyValue()
    {
        const string value = "local-value";

        var address = new EmailAddress(value);

        Assert.Equal(value, address.Value);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new EmailAddress(null!));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyOrWhitespaceValue_ThrowsArgumentException(string value)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new EmailAddress(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(" user@example.com")]
    [InlineData("user@example.com ")]
    public void Constructor_WithSurroundingWhitespace_ThrowsArgumentException(string value)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new EmailAddress(value));

        Assert.Equal("value", exception.ParamName);
    }
}

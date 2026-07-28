using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users.Value;

public sealed class StringIdentityTests
{
    [Theory]
    [InlineData("abcde")]
    [InlineData("ABCDE")]
    [InlineData("_abc_")]
    [InlineData("__abc")]
    [InlineData("abc__")]
    [InlineData("a.b_c")]
    public void Constructor_WithValidValue_PreservesValue(string value)
    {
        var identity = new StringIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithMinimumLength_AcceptsValue()
    {
        var value = new string('a', StringIdentity.MinimumLength);

        var identity = new StringIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithMaximumLength_AcceptsValue()
    {
        var value = new string('a', StringIdentity.MaximumLength);

        var identity = new StringIdentity(value);

        Assert.Equal(value, identity.Value);
    }

    [Fact]
    public void Constructor_WithMixedCase_NormalizesWithoutChangingDisplayValue()
    {
        const string value = "User.Name_01";

        var identity = new StringIdentity(value);

        Assert.Equal(value, identity.Value);
        Assert.Equal("USER.NAME_01", identity.NormalizedValue);
    }

    [Fact]
    public void Equality_WithDifferentCasing_TreatsValuesAsSameIdentity()
    {
        var first = new StringIdentity("User.Name");
        var second = new StringIdentity("user.name");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StringIdentity(null!));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValueBelowMinimumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string('a', StringIdentity.MinimumLength - 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new StringIdentity(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValueAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string('a', StringIdentity.MaximumLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new StringIdentity(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("abcd@")]
    [InlineData("abcd#")]
    [InlineData("ab-cd")]
    [InlineData("abcd ")]
    [InlineData("abcd\u00E9")]
    public void Constructor_WithDisallowedCharacter_ThrowsArgumentException(string value)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new StringIdentity(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(".abcd")]
    [InlineData("abcd.")]
    [InlineData("ab..cd")]
    public void Constructor_WithInvalidPeriodPlacement_ThrowsArgumentException(string value)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new StringIdentity(value));

        Assert.Equal("value", exception.ParamName);
    }
}

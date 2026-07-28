using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users.Value;

public sealed class ProfilePreferenceValueTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/profile")]
    public void PersonalLink_WithHttpOrHttpsUrl_AcceptsValue(string value)
    {
        var link = new PersonalLink(value);

        Assert.Equal(value, link.Value);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("/relative")]
    [InlineData("not a url")]
    public void PersonalLink_WithUnsupportedValue_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new PersonalLink(value));
    }

    [Fact]
    public void PersonalLink_AboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = $"https://example.com/{new string('a', PersonalLink.MaximumLength)}";

        Assert.Throws<ArgumentOutOfRangeException>(() => new PersonalLink(value));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("zh-TW")]
    public void LanguageTag_WithRecognizedBcp47Tag_AcceptsValue(string value)
    {
        var language = new LanguageTag(value);

        Assert.Equal(value, language.Value);
    }

    [Fact]
    public void LanguageTag_WithUnknownTag_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new LanguageTag("invalid_tag"));
    }

    [Theory]
    [InlineData("Asia/Taipei")]
    [InlineData("Etc/UTC")]
    public void TimeZoneIdentifier_WithIanaIdentifier_AcceptsValue(string value)
    {
        var timeZone = new TimeZoneIdentifier(value);

        Assert.Equal(value, timeZone.Value);
    }

    [Fact]
    public void TimeZoneIdentifier_WithUnknownIdentifier_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new TimeZoneIdentifier("Unknown/Somewhere"));
    }

    [Theory]
    [InlineData("#123456", "#123456")]
    [InlineData("#abcdef", "#ABCDEF")]
    [InlineData("#12345678", "#12345678")]
    public void ThemeColor_WithSupportedFormat_NormalizesValue(
        string value,
        string expected)
    {
        var color = new ThemeColor(value);

        Assert.Equal(expected, color.Value);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    public void ThemeColor_WithUnsupportedFormat_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new ThemeColor(value));
    }

    [Fact]
    public void FontFamily_AboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var value = new string('a', FontFamily.MaximumLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new FontFamily(value));
    }
}

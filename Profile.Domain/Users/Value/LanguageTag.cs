using System.Globalization;

namespace Profile.Domain.Users.Value;

public sealed record LanguageTag
{
    public LanguageTag(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Language tag cannot be empty or whitespace.",
                nameof(value));
        }

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Language tag cannot contain surrounding whitespace.",
                nameof(value));
        }
        if (!HasValidBcp47Syntax(value))
        {
            throw new ArgumentException(
                "Language tag must use valid BCP 47 syntax.",
                nameof(value));
        }

        try
        {
            CultureInfo.GetCultureInfo(value);
        }
        catch (CultureNotFoundException exception)
        {
            throw new ArgumentException(
                "Language tag must be a recognized BCP 47 language tag.",
                nameof(value),
                exception);
        }

        Value = value;
    }

    public string Value { get; }
    private static bool HasValidBcp47Syntax(string value)
    {
        var subtags = value.Split('-');

        if (subtags[0].Length is < 2 or > 8 ||
            !subtags[0].All(IsAsciiLetter))
        {
            return false;
        }

        return subtags
            .Skip(1)
            .All(static subtag =>
                subtag.Length is >= 1 and <= 8 &&
                subtag.All(IsAsciiLetterOrDigit));
    }

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool IsAsciiLetterOrDigit(char character) =>
        IsAsciiLetter(character) || character is >= '0' and <= '9';

}

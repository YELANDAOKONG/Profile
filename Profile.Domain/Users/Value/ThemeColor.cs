namespace Profile.Domain.Users.Value;

public sealed record ThemeColor
{
    public const int RgbLength = 7;
    public const int RgbaLength = 9;

    public ThemeColor(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length is not (RgbLength or RgbaLength) ||
            value[0] != '#' ||
            !ContainsOnlyHexadecimalCharacters(value.AsSpan(1)))
        {
            throw new ArgumentException(
                "Theme color must use #RRGGBB or #RRGGBBAA format.",
                nameof(value));
        }

        Value = value.ToUpperInvariant();
    }

    public string Value { get; }

    private static bool ContainsOnlyHexadecimalCharacters(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (character is not (>= '0' and <= '9') and
                not (>= 'A' and <= 'F') and
                not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }
}

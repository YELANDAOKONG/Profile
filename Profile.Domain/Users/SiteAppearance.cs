using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed record SiteAppearance
{
    public SiteAppearance(
        ThemeColor primaryColor,
        ThemeColor backgroundColor,
        FontFamily fontFamily)
    {
        ArgumentNullException.ThrowIfNull(primaryColor);
        ArgumentNullException.ThrowIfNull(backgroundColor);
        ArgumentNullException.ThrowIfNull(fontFamily);

        PrimaryColor = primaryColor;
        BackgroundColor = backgroundColor;
        FontFamily = fontFamily;
    }

    public ThemeColor PrimaryColor { get; }

    public ThemeColor BackgroundColor { get; }

    public FontFamily FontFamily { get; }
}

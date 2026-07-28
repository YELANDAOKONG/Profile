namespace Profile.Domain.Users.Value;

public sealed record TimeZoneIdentifier
{
    public TimeZoneIdentifier(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Time zone identifier cannot be empty or whitespace.",
                nameof(value));
        }

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Time zone identifier cannot contain surrounding whitespace.",
                nameof(value));
        }

        if (!TimeZoneInfo.TryConvertIanaIdToWindowsId(value, out _))
        {
            throw new ArgumentException(
                "Time zone identifier must be a recognized IANA time zone identifier.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}

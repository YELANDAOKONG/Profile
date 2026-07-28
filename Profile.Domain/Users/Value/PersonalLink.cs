namespace Profile.Domain.Users.Value;

public sealed record PersonalLink
{
    public const int MaximumLength = 512;

    public PersonalLink(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Personal link cannot be empty or whitespace.",
                nameof(value));
        }

        if (value.Length > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Personal link length cannot exceed {MaximumLength} characters.");
        }

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Personal link cannot contain surrounding whitespace.",
                nameof(value));
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) &&
             !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "Personal link must be an absolute HTTP or HTTPS URL.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}

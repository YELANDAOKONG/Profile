namespace Profile.Domain.Users.Value;

public sealed record EmailAddress
{
    public EmailAddress(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Email address cannot be empty or whitespace.",
                nameof(value));
        }

        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            throw new ArgumentException(
                "Email address cannot contain surrounding whitespace.",
                nameof(value));
        }

        Value = value;
        NormalizedValue = value.ToUpperInvariant();
    }

    public string Value { get; }

    public string NormalizedValue { get; }

    public bool Equals(EmailAddress? other) =>
        other is not null &&
        string.Equals(NormalizedValue, other.NormalizedValue, StringComparison.Ordinal);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(NormalizedValue);
}

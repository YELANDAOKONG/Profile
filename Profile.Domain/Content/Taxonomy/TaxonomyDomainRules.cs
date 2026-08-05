namespace Profile.Domain.Content.Taxonomy;

internal static class TaxonomyDomainRules
{
    public static void ValidateOptionalText(
        string? value,
        int maximumLength,
        string parameterName,
        string displayName)
    {
        if (value is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{displayName} cannot be empty or whitespace.",
                parameterName);
        }

        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            throw new ArgumentException(
                $"{displayName} cannot contain surrounding whitespace.",
                parameterName);
        }

        if (value.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value.Length,
                $"{displayName} length cannot exceed {maximumLength} characters.");
        }
    }

    public static void ValidateSortOrder(long sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                sortOrder,
                "Taxonomy sort order cannot be negative.");
        }
    }

    public static void ValidateTimestamps(
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (updatedAt < createdAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAt),
                updatedAt,
                "Taxonomy updated time cannot be earlier than created time.");
        }
    }

    public static void EnsureMutationTime(
        DateTimeOffset changedAt,
        DateTimeOffset updatedAt)
    {
        if (changedAt < updatedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(changedAt),
                changedAt,
                "Taxonomy change time cannot be earlier than updated time.");
        }
    }
}

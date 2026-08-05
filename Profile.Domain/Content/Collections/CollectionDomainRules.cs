namespace Profile.Domain.Content.Collections;

internal static class CollectionDomainRules
{
    public static void ValidateSortOrder(long sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                sortOrder,
                "Collection sort order cannot be negative.");
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
                "Collection updated time cannot be earlier than created time.");
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
                "Collection change time cannot be earlier than updated time.");
        }
    }
}

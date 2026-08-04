namespace Profile.Domain.Content.Value;

// The 14-day recovery period is fixed by DESIGN §6.5 for every role, so the
// value object owns the period instead of accepting it as a parameter.
public sealed record ContentDeletion
{
    public const int RecoveryPeriodDays = 14;

    private ContentDeletion(DateTimeOffset deletedAt, DateTimeOffset purgeAt)
    {
        if (purgeAt <= deletedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(purgeAt),
                purgeAt,
                "Purge time must be later than the deletion time.");
        }

        DeletedAt = deletedAt;
        PurgeAt = purgeAt;
    }

    public DateTimeOffset DeletedAt { get; }

    public DateTimeOffset PurgeAt { get; }

    public static ContentDeletion Create(DateTimeOffset deletedAt) =>
        new(deletedAt, deletedAt.AddDays(RecoveryPeriodDays));

    public static ContentDeletion Reconstitute(
        DateTimeOffset deletedAt,
        DateTimeOffset purgeAt)
    {
        // A persisted pair that disagrees with the fixed period indicates
        // corruption rather than a legacy value, so it is rejected.
        if (purgeAt != deletedAt.AddDays(RecoveryPeriodDays))
        {
            throw new ArgumentOutOfRangeException(
                nameof(purgeAt),
                purgeAt,
                "Purge time must be exactly the fixed recovery period after the deletion time.");
        }

        return new ContentDeletion(deletedAt, purgeAt);
    }

    public bool CanRestoreAt(DateTimeOffset timestamp) =>
        timestamp >= DeletedAt && timestamp < PurgeAt;

    public bool IsReadyForPurgeAt(DateTimeOffset timestamp) =>
        timestamp >= PurgeAt;
}

namespace Profile.Domain.Users;

public sealed record AccountDeletion
{
    public AccountDeletion(
        DateTimeOffset requestedAt,
        DateTimeOffset recoveryEndsAt,
        AccountDeletionContentPolicy contentPolicy)
    {
        if (!Enum.IsDefined(contentPolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentPolicy),
                contentPolicy,
                "Account deletion content policy is not supported.");
        }

        if (recoveryEndsAt <= requestedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recoveryEndsAt),
                recoveryEndsAt,
                "Recovery end time must be later than the deletion request time.");
        }

        RequestedAt = requestedAt;
        RecoveryEndsAt = recoveryEndsAt;
        ContentPolicy = contentPolicy;
    }

    public DateTimeOffset RequestedAt { get; }

    public DateTimeOffset RecoveryEndsAt { get; }

    public AccountDeletionContentPolicy ContentPolicy { get; }
}

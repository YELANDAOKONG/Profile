using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class Account
{
    private Account(
        UserIdentity id,
        StringIdentity stringId,
        AccountEmail email,
        AccountRole role,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        AccountSuspension? suspension,
        AccountBan? ban,
        AccountDeletion? deletion,
        AccountMemorialization? memorialization)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(stringId);
        ArgumentNullException.ThrowIfNull(email);

        ValidateRole(role, nameof(role));

        if (updatedAt < createdAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAt),
                updatedAt,
                "Updated time cannot be earlier than created time.");
        }

        ValidateStateTimestamp(suspension?.SuspendedAt, updatedAt, nameof(suspension));
        ValidateStateTimestamp(ban?.BannedAt, updatedAt, nameof(ban));
        ValidateStateTimestamp(deletion?.RequestedAt, updatedAt, nameof(deletion));
        ValidateStateTimestamp(
            memorialization?.MemorializedAt,
            updatedAt,
            nameof(memorialization));

        Id = id;
        StringId = stringId;
        Email = email;
        Role = role;
        Suspension = suspension;
        Ban = ban;
        Deletion = deletion;
        Memorialization = memorialization;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public UserIdentity Id { get; }

    public StringIdentity StringId { get; private set; }

    public AccountEmail Email { get; private set; }

    public AccountRole Role { get; private set; }

    // Account Status
    public AccountSuspension? Suspension { get; private set; }

    public AccountBan? Ban { get; private set; }

    public AccountDeletion? Deletion { get; private set; }

    public AccountMemorialization? Memorialization { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Account Create(
        UserIdentity id,
        StringIdentity stringId,
        AccountEmail email,
        AccountRole role,
        DateTimeOffset createdAt) =>
        new(
            id,
            stringId,
            email,
            role,
            createdAt,
            createdAt,
            null,
            null,
            null,
            null);

    public static Account Reconstitute(
        UserIdentity id,
        StringIdentity stringId,
        AccountEmail email,
        AccountRole role,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        AccountSuspension? suspension,
        AccountBan? ban,
        AccountDeletion? deletion,
        AccountMemorialization? memorialization) =>
        new(
            id,
            stringId,
            email,
            role,
            createdAt,
            updatedAt,
            suspension,
            ban,
            deletion,
            memorialization);

    public void ChangeStringId(StringIdentity stringId, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(stringId);

        EnsureCanPerformOperation(changedAt);

        if (string.Equals(StringId.Value, stringId.Value, StringComparison.Ordinal))
        {
            return;
        }

        StringId = stringId;
        UpdatedAt = changedAt;
    }

    public void ChangeEmail(EmailAddress address, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(address);

        EnsureCanPerformOperation(changedAt);

        if (string.Equals(Email.Address.Value, address.Value, StringComparison.Ordinal))
        {
            return;
        }

        var verifiedAt = Email.Address.Equals(address)
            ? Email.VerifiedAt
            : null;

        Email = new AccountEmail(address, verifiedAt);
        UpdatedAt = changedAt;
    }

    public void VerifyEmail(DateTimeOffset verifiedAt)
    {
        EnsureCanPerformOperation(verifiedAt);

        if (Email.VerifiedAt == verifiedAt)
        {
            return;
        }

        Email = Email with { VerifiedAt = verifiedAt };
        UpdatedAt = verifiedAt;
    }

    public void ChangeRole(
        Account administrator,
        AccountRole newRole,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(administrator);

        ValidateRole(newRole, nameof(newRole));

        if (administrator.Role is not AccountRole.Root)
        {
            throw new InvalidOperationException(
                "Only a Root account can change account roles.");
        }

        EnsureCanBeManagedBy(administrator, changedAt);

        if (newRole is AccountRole.Root)
        {
            throw new InvalidOperationException(
                "A Root role cannot be assigned through ordinary account administration.");
        }

        ApplyRole(newRole, changedAt);
    }

    public void ChangeRoleBySystemAdministration(
        AccountRole newRole,
        DateTimeOffset changedAt)
    {
        ValidateRole(newRole, nameof(newRole));
        EnsureMutationTime(changedAt);
        ApplyRole(newRole, changedAt);
    }

    public void Suspend(
        Account administrator,
        DateTimeOffset suspendedAt,
        DateTimeOffset? expiresAt = null,
        string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(administrator);

        EnsureCanBeManagedBy(administrator, suspendedAt);
        ApplySuspension(suspendedAt, expiresAt, reason);
    }

    public void RemoveSuspension(
        Account administrator,
        DateTimeOffset removedAt)
    {
        ArgumentNullException.ThrowIfNull(administrator);

        EnsureCanBeManagedBy(administrator, removedAt);
        RemoveSuspension(removedAt);
    }

    public void BanAccount(
        Account administrator,
        DateTimeOffset bannedAt,
        DateTimeOffset? expiresAt = null,
        string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(administrator);

        EnsureCanBeManagedBy(administrator, bannedAt);
        ApplyBan(bannedAt, expiresAt, reason);
    }

    public void RemoveBan(
        Account administrator,
        DateTimeOffset removedAt)
    {
        ArgumentNullException.ThrowIfNull(administrator);

        EnsureCanBeManagedBy(administrator, removedAt);
        RemoveBan(removedAt);
    }

    public void SuspendBySystemAdministration(
        DateTimeOffset suspendedAt,
        DateTimeOffset? expiresAt = null,
        string? reason = null) =>
        ApplySuspension(suspendedAt, expiresAt, reason);

    public void RemoveSuspensionBySystemAdministration(
        DateTimeOffset removedAt) =>
        RemoveSuspension(removedAt);

    public void BanBySystemAdministration(
        DateTimeOffset bannedAt,
        DateTimeOffset? expiresAt = null,
        string? reason = null) =>
        ApplyBan(bannedAt, expiresAt, reason);

    public void RemoveBanBySystemAdministration(
        DateTimeOffset removedAt) =>
        RemoveBan(removedAt);

    public void RequestDeletion(
        DateTimeOffset requestedAt,
        TimeSpan recoveryPeriod,
        AccountDeletionContentPolicy contentPolicy)
    {
        EnsureCanPerformOperation(requestedAt);

        if (recoveryPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recoveryPeriod),
                recoveryPeriod,
                "Recovery period must be greater than zero.");
        }

        if (Deletion is not null)
        {
            throw new InvalidOperationException(
                "The account already has a pending deletion request.");
        }

        Deletion = new AccountDeletion(
            requestedAt,
            requestedAt.Add(recoveryPeriod),
            contentPolicy);
        UpdatedAt = requestedAt;
    }

    public void Restore(DateTimeOffset restoredAt)
    {
        EnsureMutationTime(restoredAt);

        if (Deletion is null)
        {
            throw new InvalidOperationException(
                "The account does not have a pending deletion request.");
        }

        if (IsBannedAt(restoredAt))
        {
            throw new InvalidOperationException(
                "A banned account cannot restore itself.");
        }

        if (!IsDeletionPendingAt(restoredAt))
        {
            throw new InvalidOperationException(
                "The account cannot be restored after its recovery period has ended.");
        }

        Deletion = null;
        UpdatedAt = restoredAt;
    }

    public bool IsSuspendedAt(DateTimeOffset timestamp) =>
        Suspension is { } suspension &&
        timestamp >= suspension.SuspendedAt &&
        IsRestrictionActiveAt(suspension.ExpiresAt, timestamp);

    public bool IsBannedAt(DateTimeOffset timestamp) =>
        Ban is { } ban &&
        timestamp >= ban.BannedAt &&
        IsRestrictionActiveAt(ban.ExpiresAt, timestamp);

    public bool IsDeletionPendingAt(DateTimeOffset timestamp) =>
        Deletion is { } deletion &&
        timestamp >= deletion.RequestedAt &&
        timestamp < deletion.RecoveryEndsAt;

    public bool IsReadyForPermanentDeletionAt(DateTimeOffset timestamp) =>
        Deletion is { } deletion &&
        timestamp >= deletion.RecoveryEndsAt;

    public bool CanLoginAt(DateTimeOffset timestamp)
    {
        if (timestamp < CreatedAt || IsBannedAt(timestamp))
        {
            return false;
        }

        return Deletion is null || IsDeletionPendingAt(timestamp);
    }

    public bool CanPerformOperationsAt(DateTimeOffset timestamp) =>
        timestamp >= CreatedAt &&
        Deletion is null &&
        !IsSuspendedAt(timestamp) &&
        !IsBannedAt(timestamp);

    public bool CanRestoreAt(DateTimeOffset timestamp) =>
        timestamp >= CreatedAt &&
        IsDeletionPendingAt(timestamp) &&
        !IsBannedAt(timestamp);

    public bool CanManage(Account account, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(account);

        return CanPerformOperationsAt(timestamp) &&
            GetRoleRank(Role) > GetRoleRank(account.Role);
    }

    private static bool IsRestrictionActiveAt(
        DateTimeOffset? expiresAt,
        DateTimeOffset timestamp) =>
        expiresAt is null || timestamp < expiresAt.Value;

    private static int GetRoleRank(AccountRole role) =>
        role switch
        {
            AccountRole.User => 0,
            AccountRole.Administrator => 1,
            AccountRole.Root => 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Account role is not supported.")
        };

    private static void ValidateRole(AccountRole role, string parameterName)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                role,
                "Account role is not supported.");
        }
    }

    private static void ValidateStateTimestamp(
        DateTimeOffset? stateTimestamp,
        DateTimeOffset updatedAt,
        string parameterName)
    {
        if (stateTimestamp > updatedAt)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                stateTimestamp,
                "Account state cannot start after the account was last updated.");
        }
    }

    private void ApplyRole(AccountRole newRole, DateTimeOffset changedAt)
    {
        if (Role == newRole)
        {
            return;
        }

        Role = newRole;
        UpdatedAt = changedAt;
    }

    private void ApplySuspension(
        DateTimeOffset suspendedAt,
        DateTimeOffset? expiresAt,
        string? reason)
    {
        EnsureMutationTime(suspendedAt);

        Suspension = new AccountSuspension(
            suspendedAt,
            expiresAt,
            reason);
        UpdatedAt = suspendedAt;
    }

    private void RemoveSuspension(DateTimeOffset removedAt)
    {
        EnsureMutationTime(removedAt);

        if (Suspension is null)
        {
            return;
        }

        Suspension = null;
        UpdatedAt = removedAt;
    }

    private void ApplyBan(
        DateTimeOffset bannedAt,
        DateTimeOffset? expiresAt,
        string? reason)
    {
        EnsureMutationTime(bannedAt);

        Ban = new AccountBan(
            bannedAt,
            expiresAt,
            reason);
        UpdatedAt = bannedAt;
    }

    private void RemoveBan(DateTimeOffset removedAt)
    {
        EnsureMutationTime(removedAt);

        if (Ban is null)
        {
            return;
        }

        Ban = null;
        UpdatedAt = removedAt;
    }

    private void EnsureCanBeManagedBy(
        Account administrator,
        DateTimeOffset changedAt)
    {
        EnsureMutationTime(changedAt);

        if (changedAt < administrator.UpdatedAt ||
            !administrator.CanManage(this, changedAt))
        {
            throw new InvalidOperationException(
                "The acting account is not authorized to manage this account.");
        }
    }

    private void EnsureCanPerformOperation(DateTimeOffset changedAt)
    {
        EnsureMutationTime(changedAt);

        if (!CanPerformOperationsAt(changedAt))
        {
            throw new InvalidOperationException(
                "The account cannot perform this operation in its current state.");
        }
    }

    private void EnsureMutationTime(DateTimeOffset changedAt)
    {
        if (changedAt < UpdatedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(changedAt),
                changedAt,
                "Change time cannot be earlier than the account's updated time.");
        }
    }
}

using Profile.Domain.Social.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Social;

public sealed class FollowRequest
{
    private FollowRequest(
        FollowRequestIdentity id,
        UserIdentity requesterId,
        UserIdentity targetId,
        DateTimeOffset requestedAt,
        FollowRequestStatus status,
        DateTimeOffset? resolvedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(requesterId);
        ArgumentNullException.ThrowIfNull(targetId);

        if (requesterId == targetId)
        {
            throw new ArgumentException(
                "An account cannot request to follow itself.",
                nameof(targetId));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Follow request status is not supported.");
        }

        ValidateResolution(status, requestedAt, resolvedAt);

        Id = id;
        RequesterId = requesterId;
        TargetId = targetId;
        RequestedAt = requestedAt;
        Status = status;
        ResolvedAt = resolvedAt;
    }

    public FollowRequestIdentity Id { get; }

    public UserIdentity RequesterId { get; }

    public UserIdentity TargetId { get; }

    public DateTimeOffset RequestedAt { get; }

    public FollowRequestStatus Status { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public bool IsPending => Status is FollowRequestStatus.Pending;

    public static FollowRequest Create(
        UserIdentity requesterId,
        UserIdentity targetId,
        DateTimeOffset requestedAt) =>
        new(
            FollowRequestIdentity.New(),
            requesterId,
            targetId,
            requestedAt,
            FollowRequestStatus.Pending,
            null);

    public static FollowRequest Reconstitute(
        FollowRequestIdentity id,
        UserIdentity requesterId,
        UserIdentity targetId,
        DateTimeOffset requestedAt,
        FollowRequestStatus status,
        DateTimeOffset? resolvedAt) =>
        new(
            id,
            requesterId,
            targetId,
            requestedAt,
            status,
            resolvedAt);

    public Follow Approve(
        UserIdentity approverId,
        DateTimeOffset approvedAt,
        bool isBlocked)
    {
        EnsureTargetActor(approverId);
        EnsurePending();
        EnsureResolutionTime(approvedAt, nameof(approvedAt));

        if (isBlocked)
        {
            throw new InvalidOperationException(
                "A follow request cannot be approved while either account blocks the other.");
        }

        Status = FollowRequestStatus.Approved;
        ResolvedAt = approvedAt;

        return Follow.Create(RequesterId, TargetId, approvedAt);
    }

    public void Reject(UserIdentity rejecterId, DateTimeOffset rejectedAt)
    {
        EnsureTargetActor(rejecterId);
        Resolve(FollowRequestStatus.Rejected, rejectedAt, nameof(rejectedAt));
    }

    public void Cancel(UserIdentity requesterId, DateTimeOffset cancelledAt)
    {
        ArgumentNullException.ThrowIfNull(requesterId);

        if (requesterId != RequesterId)
        {
            throw new InvalidOperationException(
                "Only the requester can cancel a follow request.");
        }

        Resolve(FollowRequestStatus.Cancelled, cancelledAt, nameof(cancelledAt));
    }

    private static void ValidateResolution(
        FollowRequestStatus status,
        DateTimeOffset requestedAt,
        DateTimeOffset? resolvedAt)
    {
        if (status is FollowRequestStatus.Pending && resolvedAt is not null)
        {
            throw new ArgumentException(
                "A pending follow request cannot have a resolution time.",
                nameof(resolvedAt));
        }

        if (status is not FollowRequestStatus.Pending && resolvedAt is null)
        {
            throw new ArgumentException(
                "A resolved follow request must have a resolution time.",
                nameof(resolvedAt));
        }

        if (resolvedAt < requestedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(resolvedAt),
                resolvedAt,
                "Follow request resolution time cannot be earlier than its request time.");
        }
    }

    private void EnsureTargetActor(UserIdentity actorId)
    {
        ArgumentNullException.ThrowIfNull(actorId);

        if (actorId != TargetId)
        {
            throw new InvalidOperationException(
                "Only the target account can resolve a follow request.");
        }
    }

    private void EnsurePending()
    {
        if (!IsPending)
        {
            throw new InvalidOperationException(
                "Only a pending follow request can be resolved.");
        }
    }

    private void EnsureResolutionTime(
        DateTimeOffset resolvedAt,
        string parameterName)
    {
        if (resolvedAt < RequestedAt)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                resolvedAt,
                "Follow request resolution time cannot be earlier than its request time.");
        }
    }

    private void Resolve(
        FollowRequestStatus status,
        DateTimeOffset resolvedAt,
        string parameterName)
    {
        EnsurePending();
        EnsureResolutionTime(resolvedAt, parameterName);
        Status = status;
        ResolvedAt = resolvedAt;
    }
}

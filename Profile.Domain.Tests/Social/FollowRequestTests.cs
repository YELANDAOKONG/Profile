using Profile.Domain.Social;
using Profile.Domain.Social.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Social;

public sealed class FollowRequestTests
{
    private static readonly DateTimeOffset _requestedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithDistinctAccounts_CreatesPendingRequest()
    {
        var requesterId = UserIdentity.New();
        var targetId = UserIdentity.New();

        var request = FollowRequest.Create(
            requesterId,
            targetId,
            _requestedAt);

        Assert.NotEqual(Guid.Empty, request.Id.Value);
        Assert.Equal(requesterId, request.RequesterId);
        Assert.Equal(targetId, request.TargetId);
        Assert.Equal(_requestedAt, request.RequestedAt);
        Assert.Equal(FollowRequestStatus.Pending, request.Status);
        Assert.True(request.IsPending);
        Assert.Null(request.ResolvedAt);
    }

    [Fact]
    public void Create_WithSameAccount_ThrowsArgumentException()
    {
        var accountId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => FollowRequest.Create(accountId, accountId, _requestedAt));
    }

    [Theory]
    [InlineData(FollowRequestStatus.Approved)]
    [InlineData(FollowRequestStatus.Rejected)]
    [InlineData(FollowRequestStatus.Cancelled)]
    public void Reconstitute_WithResolvedState_PreservesState(
        FollowRequestStatus status)
    {
        var resolvedAt = _requestedAt.AddMinutes(1);

        var request = FollowRequest.Reconstitute(
            FollowRequestIdentity.New(),
            UserIdentity.New(),
            UserIdentity.New(),
            _requestedAt,
            status,
            resolvedAt);

        Assert.Equal(status, request.Status);
        Assert.False(request.IsPending);
        Assert.Equal(resolvedAt, request.ResolvedAt);
    }

    [Fact]
    public void Reconstitute_WithUnsupportedStatus_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FollowRequest.Reconstitute(
                FollowRequestIdentity.New(),
                UserIdentity.New(),
                UserIdentity.New(),
                _requestedAt,
                (FollowRequestStatus)999,
                null));
    }

    [Fact]
    public void Reconstitute_WithPendingResolutionTime_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => FollowRequest.Reconstitute(
                FollowRequestIdentity.New(),
                UserIdentity.New(),
                UserIdentity.New(),
                _requestedAt,
                FollowRequestStatus.Pending,
                _requestedAt));
    }

    [Fact]
    public void Reconstitute_WithResolvedStatusWithoutTime_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => FollowRequest.Reconstitute(
                FollowRequestIdentity.New(),
                UserIdentity.New(),
                UserIdentity.New(),
                _requestedAt,
                FollowRequestStatus.Rejected,
                null));
    }

    [Fact]
    public void Reconstitute_WithResolutionBeforeRequest_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FollowRequest.Reconstitute(
                FollowRequestIdentity.New(),
                UserIdentity.New(),
                UserIdentity.New(),
                _requestedAt,
                FollowRequestStatus.Approved,
                _requestedAt.AddTicks(-1)));
    }

    [Fact]
    public void Approve_ByTarget_ResolvesRequestAndCreatesFollow()
    {
        var requesterId = UserIdentity.New();
        var targetId = UserIdentity.New();
        var approvedAt = _requestedAt.AddMinutes(1);
        var request = FollowRequest.Create(
            requesterId,
            targetId,
            _requestedAt);

        var follow = request.Approve(targetId, approvedAt, false);

        Assert.Equal(FollowRequestStatus.Approved, request.Status);
        Assert.Equal(approvedAt, request.ResolvedAt);
        Assert.Equal(requesterId, follow.FollowerId);
        Assert.Equal(targetId, follow.FollowedId);
        Assert.Equal(approvedAt, follow.FollowedAt);
    }

    [Fact]
    public void Approve_WhileBlocked_RejectsWithoutChangingRequest()
    {
        var targetId = UserIdentity.New();
        var request = FollowRequest.Create(
            UserIdentity.New(),
            targetId,
            _requestedAt);

        Assert.Throws<InvalidOperationException>(
            () => request.Approve(
                targetId,
                _requestedAt.AddMinutes(1),
                true));

        Assert.True(request.IsPending);
        Assert.Null(request.ResolvedAt);
    }

    [Fact]
    public void Approve_ByNonTarget_ThrowsInvalidOperationException()
    {
        var request = FollowRequest.Create(
            UserIdentity.New(),
            UserIdentity.New(),
            _requestedAt);

        Assert.Throws<InvalidOperationException>(
            () => request.Approve(
                UserIdentity.New(),
                _requestedAt,
                false));
    }

    [Fact]
    public void Approve_BeforeRequestTime_ThrowsArgumentOutOfRangeException()
    {
        var targetId = UserIdentity.New();
        var request = FollowRequest.Create(
            UserIdentity.New(),
            targetId,
            _requestedAt);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => request.Approve(
                targetId,
                _requestedAt.AddTicks(-1),
                false));
    }

    [Fact]
    public void Reject_ByTarget_ResolvesRequest()
    {
        var targetId = UserIdentity.New();
        var rejectedAt = _requestedAt.AddMinutes(1);
        var request = FollowRequest.Create(
            UserIdentity.New(),
            targetId,
            _requestedAt);

        request.Reject(targetId, rejectedAt);

        Assert.Equal(FollowRequestStatus.Rejected, request.Status);
        Assert.Equal(rejectedAt, request.ResolvedAt);
    }

    [Fact]
    public void Reject_ByNonTarget_ThrowsInvalidOperationException()
    {
        var request = FollowRequest.Create(
            UserIdentity.New(),
            UserIdentity.New(),
            _requestedAt);

        Assert.Throws<InvalidOperationException>(
            () => request.Reject(UserIdentity.New(), _requestedAt));
    }

    [Fact]
    public void Cancel_ByRequester_ResolvesRequest()
    {
        var requesterId = UserIdentity.New();
        var cancelledAt = _requestedAt.AddMinutes(1);
        var request = FollowRequest.Create(
            requesterId,
            UserIdentity.New(),
            _requestedAt);

        request.Cancel(requesterId, cancelledAt);

        Assert.Equal(FollowRequestStatus.Cancelled, request.Status);
        Assert.Equal(cancelledAt, request.ResolvedAt);
    }

    [Fact]
    public void Cancel_ByNonRequester_ThrowsInvalidOperationException()
    {
        var request = FollowRequest.Create(
            UserIdentity.New(),
            UserIdentity.New(),
            _requestedAt);

        Assert.Throws<InvalidOperationException>(
            () => request.Cancel(UserIdentity.New(), _requestedAt));
    }

    [Fact]
    public void Resolve_AResolvedRequestAgain_ThrowsInvalidOperationException()
    {
        var targetId = UserIdentity.New();
        var request = FollowRequest.Create(
            UserIdentity.New(),
            targetId,
            _requestedAt);
        request.Reject(targetId, _requestedAt);

        Assert.Throws<InvalidOperationException>(
            () => request.Reject(targetId, _requestedAt));
    }

    [Fact]
    public void Create_AfterRejection_AllowsImmediateNewRequest()
    {
        var requesterId = UserIdentity.New();
        var targetId = UserIdentity.New();
        var first = FollowRequest.Create(
            requesterId,
            targetId,
            _requestedAt);
        first.Reject(targetId, _requestedAt);

        var second = FollowRequest.Create(
            requesterId,
            targetId,
            _requestedAt);

        Assert.NotEqual(first.Id, second.Id);
        Assert.True(second.IsPending);
    }
}

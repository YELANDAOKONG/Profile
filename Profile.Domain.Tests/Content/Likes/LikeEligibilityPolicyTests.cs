using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Likes;

namespace Profile.Domain.Tests.Content.Likes;

public sealed class LikeEligibilityPolicyTests
{
    [Fact]
    public void EnsureCanLikeContent_WithEligibleTarget_AllowsOperation()
    {
        LikeEligibilityPolicy.EnsureCanLikeContent(
            isTargetActiveAndPublished: true,
            isInReadingAudience: true,
            isBlockedBetweenAccounts: false);
    }

    [Fact]
    public void EnsureCanLikeContent_WithUnavailableTarget_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => LikeEligibilityPolicy.EnsureCanLikeContent(
                isTargetActiveAndPublished: false,
                isInReadingAudience: true,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void EnsureCanLikeContent_OutsideReadingAudience_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => LikeEligibilityPolicy.EnsureCanLikeContent(
                isTargetActiveAndPublished: true,
                isInReadingAudience: false,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void EnsureCanLikeContent_WhileBlocked_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => LikeEligibilityPolicy.EnsureCanLikeContent(
                isTargetActiveAndPublished: true,
                isInReadingAudience: true,
                isBlockedBetweenAccounts: true));
    }

    [Fact]
    public void EnsureCanLikeComment_WithApprovedComment_AllowsOperation()
    {
        LikeEligibilityPolicy.EnsureCanLikeComment(
            CommentStatus.Approved,
            isHostActiveAndPublished: true,
            isInHostReadingAudience: true,
            isBlockedBetweenAccounts: false);
    }

    [Theory]
    [InlineData(CommentStatus.Pending)]
    [InlineData(CommentStatus.Spam)]
    [InlineData(CommentStatus.Deleted)]
    public void EnsureCanLikeComment_WithUnavailableStatus_ThrowsArgumentException(
        CommentStatus status)
    {
        Assert.Throws<ArgumentException>(
            () => LikeEligibilityPolicy.EnsureCanLikeComment(
                status,
                isHostActiveAndPublished: true,
                isInHostReadingAudience: true,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void EnsureCanLikeComment_WithUndefinedStatus_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => LikeEligibilityPolicy.EnsureCanLikeComment(
                (CommentStatus)999,
                isHostActiveAndPublished: true,
                isInHostReadingAudience: true,
                isBlockedBetweenAccounts: false));
    }
}

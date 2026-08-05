using Profile.Domain.Content.Comments;
using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Comments;

public sealed class CommentModerationPolicyResolverTests
{
    [Fact]
    public void Resolve_WithoutOverride_ReturnsSiteDefault()
    {
        var policy = CommentModerationPolicyResolver.Resolve(
            CommentModerationPolicy.FirstComment,
            null);

        Assert.Equal(CommentModerationPolicy.FirstComment, policy);
    }

    [Fact]
    public void Resolve_WithOverride_ReturnsOverride()
    {
        var policy = CommentModerationPolicyResolver.Resolve(
            CommentModerationPolicy.FirstComment,
            CommentModerationPolicy.AllComments);

        Assert.Equal(CommentModerationPolicy.AllComments, policy);
    }

    [Theory]
    [InlineData(CommentModerationPolicy.None, false, CommentStatus.Approved)]
    [InlineData(CommentModerationPolicy.None, true, CommentStatus.Approved)]
    [InlineData(CommentModerationPolicy.FirstComment, false, CommentStatus.Pending)]
    [InlineData(CommentModerationPolicy.FirstComment, true, CommentStatus.Approved)]
    [InlineData(CommentModerationPolicy.AllComments, false, CommentStatus.Pending)]
    [InlineData(CommentModerationPolicy.AllComments, true, CommentStatus.Pending)]
    public void DetermineInitialStatus_WithPolicy_ReturnsExpectedStatus(
        CommentModerationPolicy policy,
        bool hasPreviouslyApprovedComment,
        CommentStatus expected)
    {
        var status = CommentModerationPolicyResolver.DetermineInitialStatus(
            policy,
            hasPreviouslyApprovedComment);

        Assert.Equal(expected, status);
    }

    [Fact]
    public void Resolve_WithUndefinedPolicy_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CommentModerationPolicyResolver.Resolve(
                (CommentModerationPolicy)999,
                null));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CommentModerationPolicyResolver.Resolve(
                CommentModerationPolicy.None,
                (CommentModerationPolicy)999));
    }
}

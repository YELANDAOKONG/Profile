using Profile.Domain.Content.Comments;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Comments;

public sealed class CommentAudiencePolicyTests
{
    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void CanComment_WithGlobalRestriction_ReturnsFalse(
        bool commentsAllowed,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts)
    {
        Assert.False(CanComment(
            CommenterPolicy.AllReaders,
            commentsAllowed,
            isInReadingAudience,
            isBlockedBetweenAccounts));
    }

    [Theory]
    [InlineData(CommenterPolicy.AllReaders)]
    [InlineData(CommenterPolicy.FollowersOnly)]
    [InlineData(CommenterPolicy.MutualFollowersOnly)]
    [InlineData(CommenterPolicy.AuthorOnly)]
    public void CanComment_ForAuthor_ReturnsTrue(CommenterPolicy policy)
    {
        var authorId = UserIdentity.New();

        Assert.True(CommentAudiencePolicy.CanComment(
            authorId,
            authorId,
            commentsAllowed: true,
            policy,
            isInReadingAudience: true,
            commenterFollowsAuthor: false,
            authorFollowsCommenter: false,
            isBlockedBetweenAccounts: false));
    }

    [Theory]
    [InlineData(CommenterPolicy.AllReaders, false, false, true)]
    [InlineData(CommenterPolicy.FollowersOnly, false, false, false)]
    [InlineData(CommenterPolicy.FollowersOnly, true, false, true)]
    [InlineData(CommenterPolicy.MutualFollowersOnly, true, false, false)]
    [InlineData(CommenterPolicy.MutualFollowersOnly, true, true, true)]
    [InlineData(CommenterPolicy.AuthorOnly, true, true, false)]
    public void CanComment_ForReader_AppliesCommenterScope(
        CommenterPolicy policy,
        bool commenterFollowsAuthor,
        bool authorFollowsCommenter,
        bool expected)
    {
        Assert.Equal(
            expected,
            CanComment(
                policy,
                commenterFollowsAuthor: commenterFollowsAuthor,
                authorFollowsCommenter: authorFollowsCommenter));
    }

    [Fact]
    public void CanComment_WithUndefinedPolicy_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CanComment((CommenterPolicy)999));
    }

    private static bool CanComment(
        CommenterPolicy policy,
        bool commentsAllowed = true,
        bool isInReadingAudience = true,
        bool isBlockedBetweenAccounts = false,
        bool commenterFollowsAuthor = false,
        bool authorFollowsCommenter = false) =>
        CommentAudiencePolicy.CanComment(
            UserIdentity.New(),
            UserIdentity.New(),
            commentsAllowed,
            policy,
            isInReadingAudience,
            commenterFollowsAuthor,
            authorFollowsCommenter,
            isBlockedBetweenAccounts);
}

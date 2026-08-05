using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Likes;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Likes;

public sealed class BlogCommentLikeTests
{
    private static readonly DateTimeOffset _likedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithEligibleComment_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var commentId = BlogCommentIdentity.New();

        var like = BlogCommentLike.Create(
            likerId,
            commentId,
            CommentStatus.Approved,
            isHostActiveAndPublished: true,
            isInHostReadingAudience: true,
            isBlockedBetweenAccounts: false,
            _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(commentId, like.BlogCommentId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_ForReconstitution_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var commentId = BlogCommentIdentity.New();

        var like = new BlogCommentLike(likerId, commentId, _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(commentId, like.BlogCommentId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_WithNullLiker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new BlogCommentLike(
                null!,
                BlogCommentIdentity.New(),
                _likedAt));
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new BlogCommentLike(
                UserIdentity.New(),
                null!,
                _likedAt));
    }

    [Theory]
    [InlineData(CommentStatus.Pending)]
    [InlineData(CommentStatus.Spam)]
    [InlineData(CommentStatus.Deleted)]
    public void Create_WithUnavailableComment_ThrowsArgumentException(
        CommentStatus status)
    {
        Assert.Throws<ArgumentException>(
            () => BlogCommentLike.Create(
                UserIdentity.New(),
                BlogCommentIdentity.New(),
                status,
                isHostActiveAndPublished: true,
                isInHostReadingAudience: true,
                isBlockedBetweenAccounts: false,
                _likedAt));
    }

    [Theory]
    [InlineData(false, true, false, typeof(ArgumentException))]
    [InlineData(true, false, false, typeof(InvalidOperationException))]
    [InlineData(true, true, true, typeof(InvalidOperationException))]
    public void Create_WithIneligibleHost_ThrowsExpectedException(
        bool isHostActiveAndPublished,
        bool isInHostReadingAudience,
        bool isBlockedBetweenAccounts,
        Type exceptionType)
    {
        Assert.Throws(
            exceptionType,
            () => BlogCommentLike.Create(
                UserIdentity.New(),
                BlogCommentIdentity.New(),
                CommentStatus.Approved,
                isHostActiveAndPublished,
                isInHostReadingAudience,
                isBlockedBetweenAccounts,
                _likedAt));
    }
}

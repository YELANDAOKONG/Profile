using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Likes;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Likes;

public sealed class PostCommentLikeTests
{
    private static readonly DateTimeOffset _likedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithEligibleComment_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var commentId = PostCommentIdentity.New();

        var like = PostCommentLike.Create(
            likerId,
            commentId,
            CommentStatus.Approved,
            isHostActiveAndPublished: true,
            isInHostReadingAudience: true,
            isBlockedBetweenAccounts: false,
            _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(commentId, like.PostCommentId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_ForReconstitution_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var commentId = PostCommentIdentity.New();

        var like = new PostCommentLike(likerId, commentId, _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(commentId, like.PostCommentId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_WithNullLiker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PostCommentLike(
                null!,
                PostCommentIdentity.New(),
                _likedAt));
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PostCommentLike(
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
            () => PostCommentLike.Create(
                UserIdentity.New(),
                PostCommentIdentity.New(),
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
            () => PostCommentLike.Create(
                UserIdentity.New(),
                PostCommentIdentity.New(),
                CommentStatus.Approved,
                isHostActiveAndPublished,
                isInHostReadingAudience,
                isBlockedBetweenAccounts,
                _likedAt));
    }
}

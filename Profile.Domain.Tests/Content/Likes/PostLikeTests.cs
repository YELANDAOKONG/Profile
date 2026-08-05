using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Likes;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Likes;

public sealed class PostLikeTests
{
    private static readonly DateTimeOffset _likedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithEligibleTarget_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var postId = PostIdentity.New();

        var like = PostLike.Create(
            likerId,
            postId,
            isTargetActiveAndPublished: true,
            isInReadingAudience: true,
            isBlockedBetweenAccounts: false,
            _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(postId, like.PostId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_ForReconstitution_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var postId = PostIdentity.New();

        var like = new PostLike(likerId, postId, _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(postId, like.PostId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_WithNullLiker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PostLike(null!, PostIdentity.New(), _likedAt));
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PostLike(UserIdentity.New(), null!, _likedAt));
    }

    [Theory]
    [InlineData(false, true, false, typeof(ArgumentException))]
    [InlineData(true, false, false, typeof(InvalidOperationException))]
    [InlineData(true, true, true, typeof(InvalidOperationException))]
    public void Create_WithIneligibleTarget_ThrowsExpectedException(
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        Type exceptionType)
    {
        Assert.Throws(
            exceptionType,
            () => PostLike.Create(
                UserIdentity.New(),
                PostIdentity.New(),
                isTargetActiveAndPublished,
                isInReadingAudience,
                isBlockedBetweenAccounts,
                _likedAt));
    }
}

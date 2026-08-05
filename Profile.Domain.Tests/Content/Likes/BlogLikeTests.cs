using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Likes;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Likes;

public sealed class BlogLikeTests
{
    private static readonly DateTimeOffset _likedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithEligibleTarget_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var blogId = BlogIdentity.New();

        var like = BlogLike.Create(
            likerId,
            blogId,
            isTargetActiveAndPublished: true,
            isInReadingAudience: true,
            isBlockedBetweenAccounts: false,
            _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(blogId, like.BlogId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_ForReconstitution_PreservesRelationship()
    {
        var likerId = UserIdentity.New();
        var blogId = BlogIdentity.New();

        var like = new BlogLike(likerId, blogId, _likedAt);

        Assert.Equal(likerId, like.LikerId);
        Assert.Equal(blogId, like.BlogId);
        Assert.Equal(_likedAt, like.LikedAt);
    }

    [Fact]
    public void Constructor_WithNullLiker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new BlogLike(null!, BlogIdentity.New(), _likedAt));
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new BlogLike(UserIdentity.New(), null!, _likedAt));
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
            () => BlogLike.Create(
                UserIdentity.New(),
                BlogIdentity.New(),
                isTargetActiveAndPublished,
                isInReadingAudience,
                isBlockedBetweenAccounts,
                _likedAt));
    }
}

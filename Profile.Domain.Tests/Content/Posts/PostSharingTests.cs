using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Posts;

public sealed class PostSharingTests
{
    [Fact]
    public void CreateQuote_WithPublicTarget_CreatesDraftReferencingTarget()
    {
        var target = CreateShareablePost();

        var quote = CreateQuote(target, UserIdentity.New());

        Assert.Equal(target.Id, quote.QuotedPostId);
        Assert.Equal(PublicationStatus.Draft, quote.Publication.Status);
        Assert.NotNull(quote.Body);
    }

    [Fact]
    public void CreateQuote_WithOwnPublicPost_AllowsValue()
    {
        var authorId = UserIdentity.New();
        var target = CreateShareablePost(authorId);

        var quote = CreateQuote(target, authorId);

        Assert.Equal(authorId, quote.AuthorId);
        Assert.Equal(target.Id, quote.QuotedPostId);
    }

    [Fact]
    public void CreateQuote_WithoutOwnBodyOrMedia_ThrowsArgumentException()
    {
        var target = CreateShareablePost();

        Assert.Throws<ArgumentException>(
            () => Post.CreateQuote(
                PostIdentity.New(),
                UserIdentity.New(),
                null,
                [],
                ContentVisibility.Public,
                AudienceRestrictionMode.Blacklist,
                [],
                target,
                isBlockedBetweenAuthors: false,
                commentsAllowed: true,
                CommenterPolicy.AllReaders,
                [],
                PostTestFactory.BaseTime));
    }

    [Fact]
    public void CreateQuote_WhileBlocked_ThrowsInvalidOperationException()
    {
        var target = CreateShareablePost();

        Assert.Throws<InvalidOperationException>(
            () => CreateQuote(
                target,
                UserIdentity.New(),
                isBlockedBetweenAuthors: true));
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Deleted")]
    [InlineData("Followers")]
    [InlineData("Blacklist")]
    [InlineData("Whitelist")]
    public void CreateQuote_WithRestrictedOrUnavailableTarget_ThrowsArgumentException(
        string scenario)
    {
        var target = CreateNonShareablePost(scenario);

        Assert.Throws<ArgumentException>(
            () => CreateQuote(target, UserIdentity.New()));
    }

    [Fact]
    public void PostRepost_Create_WithPublicTarget_PreservesRelationship()
    {
        var target = CreateShareablePost();
        var reposterId = UserIdentity.New();
        var repostedAt = PostTestFactory.BaseTime.AddMinutes(1);

        var repost = PostRepost.Create(
            reposterId,
            target,
            isBlockedBetweenAuthors: false,
            repostedAt);

        Assert.NotEqual(Guid.Empty, repost.Id.Value);
        Assert.Equal(reposterId, repost.ReposterId);
        Assert.Equal(target.Id, repost.PostId);
        Assert.Equal(repostedAt, repost.RepostedAt);
    }

    [Fact]
    public void PostRepost_CreateOwnPost_AllowsValue()
    {
        var authorId = UserIdentity.New();
        var target = CreateShareablePost(authorId);

        var repost = PostRepost.Create(
            authorId,
            target,
            isBlockedBetweenAuthors: false,
            PostTestFactory.BaseTime.AddMinutes(1));

        Assert.Equal(authorId, repost.ReposterId);
    }

    [Fact]
    public void PostRepost_CreateSameTargetMultipleTimes_CreatesDistinctRelationships()
    {
        var target = CreateShareablePost();
        var reposterId = UserIdentity.New();

        var first = PostRepost.Create(
            reposterId,
            target,
            false,
            PostTestFactory.BaseTime.AddMinutes(1));
        var second = PostRepost.Create(
            reposterId,
            target,
            false,
            PostTestFactory.BaseTime.AddMinutes(2));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(first.PostId, second.PostId);
        Assert.Equal(first.ReposterId, second.ReposterId);
    }

    [Fact]
    public void PostRepost_CreateWhileBlocked_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => PostRepost.Create(
                UserIdentity.New(),
                CreateShareablePost(),
                true,
                PostTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void PostRepost_CreateWithRestrictedTarget_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => PostRepost.Create(
                UserIdentity.New(),
                CreateNonShareablePost("Blacklist"),
                false,
                PostTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void IsPubliclyShareable_AfterVisibilityReduction_ReturnsFalse()
    {
        var target = CreateShareablePost();

        target.ChangeVisibility(
            ContentVisibility.Followers,
            PostTestFactory.BaseTime.AddMinutes(1));

        Assert.False(target.IsPubliclyShareable);
    }

    [Fact]
    public void IsPubliclyShareable_AfterDeleteAndRestore_RecoversValue()
    {
        var target = CreateShareablePost();
        var deletedAt = PostTestFactory.BaseTime.AddMinutes(1);
        target.Delete(deletedAt);

        Assert.False(target.IsPubliclyShareable);

        target.Restore(deletedAt.AddMinutes(1));

        Assert.True(target.IsPubliclyShareable);
    }

    private static Post CreateQuote(
        Post target,
        UserIdentity authorId,
        bool isBlockedBetweenAuthors = false) =>
        Post.CreateQuote(
            PostIdentity.New(),
            authorId,
            PostTestFactory.CreateBody("Quote commentary"),
            [],
            ContentVisibility.Public,
            AudienceRestrictionMode.Blacklist,
            [],
            target,
            isBlockedBetweenAuthors,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            [],
            PostTestFactory.BaseTime);

    private static Post CreateShareablePost(UserIdentity? authorId = null) =>
        PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                PostTestFactory.BaseTime,
                PostTestFactory.BaseTime),
            authorId: authorId);

    private static Post CreateNonShareablePost(string scenario)
    {
        var published = Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            PostTestFactory.BaseTime,
            PostTestFactory.BaseTime);

        return scenario switch
        {
            "Draft" => PostTestFactory.CreatePost(),
            "Deleted" => PostTestFactory.ReconstitutePost(
                published,
                ContentDeletion.Create(PostTestFactory.BaseTime),
                updatedAt: PostTestFactory.BaseTime),
            "Followers" => PostTestFactory.ReconstitutePost(
                published,
                visibility: ContentVisibility.Followers),
            "Blacklist" => PostTestFactory.ReconstitutePost(
                published,
                audienceAccountIds: [UserIdentity.New()]),
            "Whitelist" => PostTestFactory.ReconstitutePost(
                published,
                audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
                audienceAccountIds: [UserIdentity.New()]),
            _ => throw new ArgumentOutOfRangeException(
                nameof(scenario),
                scenario,
                "Post sharing scenario is not supported.")
        };
    }
}

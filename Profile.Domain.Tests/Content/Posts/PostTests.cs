using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Posts;

public sealed class PostTests
{
    [Fact]
    public void Create_WithCompleteState_CreatesDraftAndPreservesValues()
    {
        var id = PostIdentity.New();
        var authorId = UserIdentity.New();
        var body = PostTestFactory.CreateBody();
        var media = PostTestFactory.CreateMedia("Description");
        var audienceAccountId = UserIdentity.New();
        var tagId = PostTagIdentity.New();

        var post = Post.Create(
            id,
            authorId,
            body,
            [media],
            ContentVisibility.Followers,
            AudienceRestrictionMode.Whitelist,
            [audienceAccountId],
            commentsAllowed: false,
            CommenterPolicy.MutualFollowersOnly,
            [tagId],
            PostTestFactory.BaseTime);

        Assert.Equal(id, post.Id);
        Assert.Equal(authorId, post.AuthorId);
        Assert.Same(body, post.Body);
        Assert.Equal([media], post.Media);
        Assert.Equal(ContentVisibility.Followers, post.Visibility);
        Assert.Equal(
            AudienceRestrictionMode.Whitelist,
            post.AudienceRestrictionMode);
        Assert.Equal([audienceAccountId], post.AudienceAccountIds);
        Assert.Null(post.QuotedPostId);
        Assert.False(post.CommentsAllowed);
        Assert.Equal(CommenterPolicy.MutualFollowersOnly, post.CommenterPolicy);
        Assert.Equal([tagId], post.TagIds);
        Assert.Equal(PublicationStatus.Draft, post.Publication.Status);
        Assert.Null(post.Deletion);
        Assert.Equal(PostTestFactory.BaseTime, post.CreatedAt);
        Assert.Equal(PostTestFactory.BaseTime, post.UpdatedAt);
    }

    [Fact]
    public void Create_WithBodyOnly_AllowsDraft()
    {
        var post = CreatePost(PostTestFactory.CreateBody(), []);

        Assert.NotNull(post.Body);
        Assert.Empty(post.Media);
    }

    [Fact]
    public void Create_WithMediaOnly_AllowsDraft()
    {
        var post = CreatePost(null, [PostTestFactory.CreateMedia()]);

        Assert.Null(post.Body);
        Assert.Single(post.Media);
    }

    [Fact]
    public void Create_WithoutBodyOrMedia_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreatePost(null, []));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void Create_WithEmptyOrWhitespaceBody_ThrowsArgumentException(
        string source)
    {
        Assert.Throws<ArgumentException>(
            () => CreatePost(PostTestFactory.CreateBody(source), []));
    }

    [Fact]
    public void Create_WithMaximumBodyLength_AllowsValue()
    {
        var body = PostTestFactory.CreateBody(
            new string('a', Post.MaximumBodyLength));

        var post = CreatePost(body, []);

        Assert.Equal(Post.MaximumBodyLength, post.Body?.Source.Length);
    }

    [Fact]
    public void Create_WithBodyOverMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var body = PostTestFactory.CreateBody(
            new string('a', Post.MaximumBodyLength + 1));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePost(body, []));
    }

    [Fact]
    public void Create_WithMaximumMediaCount_AllowsValue()
    {
        var media = Enumerable.Range(0, Post.MaximumMediaCount)
            .Select(_ => PostTestFactory.CreateMedia())
            .ToArray();

        var post = CreatePost(null, media);

        Assert.Equal(Post.MaximumMediaCount, post.Media.Count);
    }

    [Fact]
    public void Create_WithTooManyMedia_ThrowsArgumentOutOfRangeException()
    {
        var media = Enumerable.Range(0, Post.MaximumMediaCount + 1)
            .Select(_ => PostTestFactory.CreateMedia())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePost(null, media));
    }

    [Fact]
    public void Create_WithNullMediaItem_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreatePost(null, [PostTestFactory.CreateMedia(), null!]));
    }

    [Fact]
    public void Create_WithDuplicateMediaIdentity_ThrowsArgumentException()
    {
        var mediaId = MediaItemIdentity.New();
        MediaReference[] media =
        [
            new(mediaId, "First use"),
            new(mediaId, "Second use")
        ];

        Assert.Throws<ArgumentException>(() => CreatePost(null, media));
    }

    [Fact]
    public void Create_CopiesMediaCollection()
    {
        List<MediaReference> media = [PostTestFactory.CreateMedia()];
        var post = CreatePost(null, media);

        media.Add(PostTestFactory.CreateMedia());

        Assert.Single(post.Media);
    }

    [Fact]
    public void Create_WithMaximumAudienceAccountCount_AllowsValue()
    {
        var audienceAccountIds = Enumerable
            .Range(0, Post.MaximumAudienceAccountCount)
            .Select(_ => UserIdentity.New())
            .ToArray();

        var post = CreatePost(audienceAccountIds: audienceAccountIds);

        Assert.Equal(
            Post.MaximumAudienceAccountCount,
            post.AudienceAccountIds.Count);
    }

    [Fact]
    public void Create_WithTooManyAudienceAccounts_ThrowsArgumentOutOfRangeException()
    {
        var audienceAccountIds = Enumerable
            .Range(0, Post.MaximumAudienceAccountCount + 1)
            .Select(_ => UserIdentity.New())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePost(audienceAccountIds: audienceAccountIds));
    }

    [Fact]
    public void Create_WithDuplicateAudienceAccount_ThrowsArgumentException()
    {
        var accountId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreatePost(audienceAccountIds: [accountId, accountId]));
    }

    [Fact]
    public void Create_WithAuthorInAudienceAccounts_ThrowsArgumentException()
    {
        var authorId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreatePost(
                authorId: authorId,
                audienceAccountIds: [authorId]));
    }

    [Fact]
    public void Create_WithMaximumTagCount_AllowsValue()
    {
        var tagIds = Enumerable.Range(0, Post.MaximumTagCount)
            .Select(_ => PostTagIdentity.New())
            .ToArray();

        var post = CreatePost(tagIds: tagIds);

        Assert.Equal(Post.MaximumTagCount, post.TagIds.Count);
    }

    [Fact]
    public void Create_WithTooManyTags_ThrowsArgumentOutOfRangeException()
    {
        var tagIds = Enumerable.Range(0, Post.MaximumTagCount + 1)
            .Select(_ => PostTagIdentity.New())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePost(tagIds: tagIds));
    }

    [Fact]
    public void Create_WithDuplicateTag_ThrowsArgumentException()
    {
        var tagId = PostTagIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreatePost(tagIds: [tagId, tagId]));
    }

    [Fact]
    public void Reconstitute_WithQuotedIdentityEqualToOwn_ThrowsArgumentException()
    {
        var id = PostIdentity.New();

        Assert.Throws<ArgumentException>(
            () => Post.Reconstitute(
                id,
                UserIdentity.New(),
                PostTestFactory.CreateBody(),
                [],
                ContentVisibility.Public,
                AudienceRestrictionMode.Blacklist,
                [],
                id,
                commentsAllowed: true,
                CommenterPolicy.AllReaders,
                [],
                Publication.CreateDraft(),
                null,
                PostTestFactory.BaseTime,
                PostTestFactory.BaseTime));
    }

    [Theory]
    [InlineData(999, 0, 0)]
    [InlineData(0, 999, 0)]
    [InlineData(0, 0, 999)]
    public void Create_WithUnsupportedEnum_ThrowsArgumentOutOfRangeException(
        int visibility,
        int mode,
        int commenterPolicy)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePost(
                visibility: (ContentVisibility)visibility,
                audienceRestrictionMode: (AudienceRestrictionMode)mode,
                commenterPolicy: (CommenterPolicy)commenterPolicy));
    }

    private static Post CreatePost(
        ContentBody? body = null,
        IEnumerable<MediaReference>? media = null,
        UserIdentity? authorId = null,
        ContentVisibility visibility = ContentVisibility.Public,
        AudienceRestrictionMode audienceRestrictionMode =
            AudienceRestrictionMode.Blacklist,
        IEnumerable<UserIdentity>? audienceAccountIds = null,
        CommenterPolicy commenterPolicy = CommenterPolicy.AllReaders,
        IEnumerable<PostTagIdentity>? tagIds = null) =>
        Post.Create(
            PostIdentity.New(),
            authorId ?? UserIdentity.New(),
            body ?? (media is null ? PostTestFactory.CreateBody() : null),
            media ?? [],
            visibility,
            audienceRestrictionMode,
            audienceAccountIds ?? [],
            commentsAllowed: true,
            commenterPolicy,
            tagIds ?? [],
            PostTestFactory.BaseTime);
}

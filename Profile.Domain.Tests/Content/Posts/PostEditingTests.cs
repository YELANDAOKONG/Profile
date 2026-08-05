using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Posts;

public sealed class PostEditingTests
{
    [Fact]
    public void UpdateContent_FromDraft_ReplacesBodyAndMedia()
    {
        var post = PostTestFactory.CreatePost();
        var body = PostTestFactory.CreateBody("Changed body");
        var media = PostTestFactory.CreateMedia();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.UpdateContent(body, [media], changedAt);

        Assert.Same(body, post.Body);
        Assert.Equal([media], post.Media);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_ToMediaOnly_AllowsValue()
    {
        var post = PostTestFactory.CreatePost();

        post.UpdateContent(
            null,
            [PostTestFactory.CreateMedia()],
            PostTestFactory.BaseTime.AddMinutes(1));

        Assert.Null(post.Body);
        Assert.Single(post.Media);
    }

    [Fact]
    public void UpdateContent_ToEmpty_ThrowsAndPreservesContent()
    {
        var post = PostTestFactory.CreatePost();
        var body = post.Body;

        Assert.Throws<ArgumentException>(
            () => post.UpdateContent(
                null,
                [],
                PostTestFactory.BaseTime.AddMinutes(1)));

        Assert.Same(body, post.Body);
        Assert.Empty(post.Media);
        Assert.Equal(PostTestFactory.BaseTime, post.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_WhilePublished_ThrowsInvalidOperationException()
    {
        var post = CreatePublishedPost();

        Assert.Throws<InvalidOperationException>(
            () => post.UpdateContent(
                PostTestFactory.CreateBody("Changed"),
                [],
                PostTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void UpdateContent_AfterUnpublishing_AllowsValue()
    {
        var post = CreatePublishedPost();
        post.UnpublishToDraft(PostTestFactory.BaseTime.AddMinutes(1));
        var body = PostTestFactory.CreateBody("Changed");

        post.UpdateContent(body, [], PostTestFactory.BaseTime.AddMinutes(2));

        Assert.Same(body, post.Body);
        Assert.Equal(PostTestFactory.BaseTime.AddMinutes(2), post.UpdatedAt);
    }

    [Fact]
    public void ChangeVisibility_WhilePublished_AllowsValue()
    {
        var post = CreatePublishedPost();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.ChangeVisibility(ContentVisibility.Followers, changedAt);

        Assert.Equal(ContentVisibility.Followers, post.Visibility);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void ChangeAudienceRestriction_WhilePublished_AllowsValue()
    {
        var post = CreatePublishedPost();
        var accountId = UserIdentity.New();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.ChangeAudienceRestriction(
            AudienceRestrictionMode.Whitelist,
            [accountId],
            changedAt);

        Assert.Equal(
            AudienceRestrictionMode.Whitelist,
            post.AudienceRestrictionMode);
        Assert.Equal([accountId], post.AudienceAccountIds);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscussion_WhilePublished_AllowsValue()
    {
        var post = CreatePublishedPost();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.ChangeDiscussion(
            commentsAllowed: false,
            CommenterPolicy.AuthorOnly,
            changedAt);

        Assert.False(post.CommentsAllowed);
        Assert.Equal(CommenterPolicy.AuthorOnly, post.CommenterPolicy);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void ChangeTags_WhilePublished_AllowsValue()
    {
        var post = CreatePublishedPost();
        var tagId = PostTagIdentity.New();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.ChangeTags([tagId], changedAt);

        Assert.Equal([tagId], post.TagIds);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void ChangeAudienceRestriction_WithAuthor_ThrowsAndPreservesState()
    {
        var post = PostTestFactory.CreatePost();

        Assert.Throws<ArgumentException>(
            () => post.ChangeAudienceRestriction(
                AudienceRestrictionMode.Whitelist,
                [post.AuthorId],
                PostTestFactory.BaseTime.AddMinutes(1)));

        Assert.Equal(
            AudienceRestrictionMode.Blacklist,
            post.AudienceRestrictionMode);
        Assert.Empty(post.AudienceAccountIds);
        Assert.Equal(PostTestFactory.BaseTime, post.UpdatedAt);
    }

    [Fact]
    public void ChangeCommentModerationPolicyOverride_SetsAndClearsOverride()
    {
        var post = PostTestFactory.CreatePost();

        post.ChangeCommentModerationPolicyOverride(
            CommentModerationPolicy.AllComments,
            PostTestFactory.BaseTime.AddMinutes(1));

        Assert.Equal(
            CommentModerationPolicy.AllComments,
            post.CommentModerationPolicyOverride);

        post.ChangeCommentModerationPolicyOverride(
            null,
            PostTestFactory.BaseTime.AddMinutes(2));

        Assert.Null(post.CommentModerationPolicyOverride);
    }

    [Theory]
    [InlineData(nameof(Post.UpdateContent))]
    [InlineData(nameof(Post.ChangeVisibility))]
    [InlineData(nameof(Post.ChangeAudienceRestriction))]
    [InlineData(nameof(Post.ChangeDiscussion))]
    [InlineData(nameof(Post.ChangeCommentModerationPolicyOverride))]
    [InlineData(nameof(Post.ChangeTags))]
    public void ChangeOperation_WhenDeleted_ThrowsInvalidOperationException(
        string operation)
    {
        var deletedAt = PostTestFactory.BaseTime.AddMinutes(1);
        var post = PostTestFactory.ReconstitutePost(
            deletion: ContentDeletion.Create(deletedAt),
            updatedAt: deletedAt);

        Assert.Throws<InvalidOperationException>(
            () => InvokeChange(post, operation, deletedAt));
    }

    [Theory]
    [InlineData(nameof(Post.UpdateContent))]
    [InlineData(nameof(Post.ChangeVisibility))]
    [InlineData(nameof(Post.ChangeAudienceRestriction))]
    [InlineData(nameof(Post.ChangeDiscussion))]
    [InlineData(nameof(Post.ChangeCommentModerationPolicyOverride))]
    [InlineData(nameof(Post.ChangeTags))]
    public void ChangeOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var post = PostTestFactory.CreatePost();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeChange(
                post,
                operation,
                PostTestFactory.BaseTime.AddTicks(-1)));
    }

    private static Post CreatePublishedPost() =>
        PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                PostTestFactory.BaseTime,
                PostTestFactory.BaseTime));

    private static void InvokeChange(
        Post post,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Post.UpdateContent):
                post.UpdateContent(PostTestFactory.CreateBody(), [], changedAt);
                break;
            case nameof(Post.ChangeVisibility):
                post.ChangeVisibility(ContentVisibility.Private, changedAt);
                break;
            case nameof(Post.ChangeAudienceRestriction):
                post.ChangeAudienceRestriction(
                    AudienceRestrictionMode.Whitelist,
                    [],
                    changedAt);
                break;
            case nameof(Post.ChangeDiscussion):
                post.ChangeDiscussion(false, CommenterPolicy.AuthorOnly, changedAt);
                break;
            case nameof(Post.ChangeCommentModerationPolicyOverride):
                post.ChangeCommentModerationPolicyOverride(
                    CommentModerationPolicy.AllComments,
                    changedAt);
                break;
            case nameof(Post.ChangeTags):
                post.ChangeTags([PostTagIdentity.New()], changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Post change operation is not supported.");
        }
    }
}

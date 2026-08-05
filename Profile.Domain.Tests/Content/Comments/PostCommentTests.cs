using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Comments;
using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Comments;

public sealed class PostCommentTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesValuesAndCopiesMedia()
    {
        var id = PostCommentIdentity.New();
        var authorId = UserIdentity.New();
        var postId = PostIdentity.New();
        var body = CreateBody("Comment body", ContentFormat.Markdown);
        var media = new List<MediaReference> { CreateMedia() };

        var comment = PostComment.Create(
            id,
            authorId,
            postId,
            null,
            body,
            media,
            CommentModerationPolicy.None,
            hasPreviouslyApprovedComment: false,
            _createdAt);
        media.Add(CreateMedia());

        Assert.Equal(id, comment.Id);
        Assert.Equal(authorId, comment.AuthorId);
        Assert.Equal(postId, comment.PostId);
        Assert.Null(comment.ParentCommentId);
        Assert.Same(body, comment.Body);
        Assert.Single(comment.Media);
        Assert.Equal(CommentStatus.Approved, comment.Status);
        Assert.Equal(_createdAt, comment.CreatedAt);
        Assert.False(comment.IsPlaceholder);
    }

    [Theory]
    [InlineData(CommentModerationPolicy.None, false, CommentStatus.Approved)]
    [InlineData(CommentModerationPolicy.FirstComment, false, CommentStatus.Pending)]
    [InlineData(CommentModerationPolicy.FirstComment, true, CommentStatus.Approved)]
    [InlineData(CommentModerationPolicy.AllComments, true, CommentStatus.Pending)]
    public void Create_WithModerationPolicy_AssignsInitialStatus(
        CommentModerationPolicy policy,
        bool hasPreviouslyApprovedComment,
        CommentStatus expected)
    {
        var comment = CreateComment(
            policy: policy,
            hasPreviouslyApprovedComment: hasPreviouslyApprovedComment);

        Assert.Equal(expected, comment.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void Create_WithBlankBody_ThrowsArgumentException(string source)
    {
        Assert.Throws<ArgumentException>(
            () => CreateComment(body: CreateBody(source)));
    }

    [Fact]
    public void Create_WithMaximumBodyLength_AllowsValue()
    {
        var comment = CreateComment(
            body: CreateBody(new string('a', PostComment.MaximumBodyLength)));

        Assert.Equal(PostComment.MaximumBodyLength, comment.Body?.Source.Length);
    }

    [Fact]
    public void Create_WithBodyOverMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateComment(
                body: CreateBody(
                    new string('a', PostComment.MaximumBodyLength + 1))));
    }

    [Fact]
    public void Create_WithMaximumMediaCount_AllowsValue()
    {
        var media = Enumerable.Range(0, PostComment.MaximumMediaCount)
            .Select(static _ => CreateMedia())
            .ToArray();

        Assert.Equal(
            PostComment.MaximumMediaCount,
            CreateComment(media: media).Media.Count);
    }

    [Fact]
    public void Create_WithTooManyMedia_ThrowsArgumentOutOfRangeException()
    {
        var media = Enumerable.Range(0, PostComment.MaximumMediaCount + 1)
            .Select(static _ => CreateMedia())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateComment(media: media));
    }

    [Fact]
    public void Create_WithDuplicateMediaIdentity_ThrowsArgumentException()
    {
        var mediaId = MediaItemIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreateComment(
                media:
                [
                    new MediaReference(mediaId, "First"),
                    new MediaReference(mediaId, "Second")
                ]));
    }

    [Fact]
    public void Create_WithNullMediaItem_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateComment(media: [CreateMedia(), null!]));
    }

    [Theory]
    [InlineData(CommentStatus.Approved)]
    [InlineData(CommentStatus.Deleted)]
    public void CreateReply_ToApprovedOrDeletedParent_AllowsValue(
        CommentStatus parentStatus)
    {
        var postId = PostIdentity.New();
        var parent = ReconstituteAtStatus(parentStatus, postId: postId);

        var reply = CreateComment(postId: postId, parentComment: parent);

        Assert.Equal(parent.Id, reply.ParentCommentId);
    }

    [Theory]
    [InlineData(CommentStatus.Pending)]
    [InlineData(CommentStatus.Spam)]
    public void CreateReply_ToHiddenParent_ThrowsInvalidOperationException(
        CommentStatus parentStatus)
    {
        var postId = PostIdentity.New();
        var parent = ReconstituteAtStatus(parentStatus, postId: postId);

        Assert.Throws<InvalidOperationException>(
            () => CreateComment(postId: postId, parentComment: parent));
    }

    [Fact]
    public void CreateReply_ToParentFromDifferentHost_ThrowsArgumentException()
    {
        var parent = ReconstituteAtStatus(CommentStatus.Approved);

        Assert.Throws<ArgumentException>(
            () => CreateComment(parentComment: parent));
    }

    [Fact]
    public void CreateReply_WithOwnIdentityAsParent_ThrowsArgumentException()
    {
        var id = PostCommentIdentity.New();
        var postId = PostIdentity.New();
        var parent = ReconstituteAtStatus(
            CommentStatus.Approved,
            id,
            postId);

        Assert.Throws<ArgumentException>(
            () => CreateComment(id, postId, parent));
    }

    [Fact]
    public void Reconstitute_DeletedPlaceholder_AllowsEmptyContent()
    {
        var comment = ReconstituteAtStatus(CommentStatus.Deleted);

        Assert.Null(comment.Body);
        Assert.Empty(comment.Media);
        Assert.True(comment.IsPlaceholder);
    }

    [Fact]
    public void Reconstitute_DeletedWithContent_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => PostComment.Reconstitute(
                PostCommentIdentity.New(),
                UserIdentity.New(),
                PostIdentity.New(),
                null,
                CreateBody(),
                [],
                CommentStatus.Deleted,
                _createdAt));
    }

    [Fact]
    public void Reconstitute_ActiveWithoutBody_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => PostComment.Reconstitute(
                PostCommentIdentity.New(),
                UserIdentity.New(),
                PostIdentity.New(),
                null,
                null,
                [],
                CommentStatus.Approved,
                _createdAt));
    }

    [Theory]
    [InlineData(CommentStatus.Pending)]
    [InlineData(CommentStatus.Spam)]
    public void Approve_FromPendingOrSpam_ChangesStatus(CommentStatus status)
    {
        var comment = ReconstituteAtStatus(status);

        comment.Approve();

        Assert.Equal(CommentStatus.Approved, comment.Status);
    }

    [Theory]
    [InlineData(CommentStatus.Approved)]
    [InlineData(CommentStatus.Deleted)]
    public void Approve_FromInvalidStatus_ThrowsInvalidOperationException(
        CommentStatus status)
    {
        var comment = ReconstituteAtStatus(status);

        Assert.Throws<InvalidOperationException>(comment.Approve);
    }

    [Theory]
    [InlineData(CommentStatus.Pending)]
    [InlineData(CommentStatus.Approved)]
    public void MarkAsSpam_FromPendingOrApproved_ChangesStatus(
        CommentStatus status)
    {
        var comment = ReconstituteAtStatus(status);

        comment.MarkAsSpam();

        Assert.Equal(CommentStatus.Spam, comment.Status);
    }

    [Theory]
    [InlineData(CommentStatus.Spam)]
    [InlineData(CommentStatus.Deleted)]
    public void MarkAsSpam_FromInvalidStatus_ThrowsInvalidOperationException(
        CommentStatus status)
    {
        var comment = ReconstituteAtStatus(status);

        Assert.Throws<InvalidOperationException>(comment.MarkAsSpam);
    }

    [Theory]
    [InlineData(CommentStatus.Pending)]
    [InlineData(CommentStatus.Approved)]
    [InlineData(CommentStatus.Spam)]
    public void Delete_FromActiveStatus_CreatesTerminalPlaceholder(
        CommentStatus status)
    {
        var comment = ReconstituteAtStatus(status, media: [CreateMedia()]);

        comment.Delete();

        Assert.Equal(CommentStatus.Deleted, comment.Status);
        Assert.Null(comment.Body);
        Assert.Empty(comment.Media);
        Assert.True(comment.IsPlaceholder);
        Assert.Throws<InvalidOperationException>(comment.Delete);
    }

    private static PostComment CreateComment(
        PostCommentIdentity? id = null,
        PostIdentity? postId = null,
        PostComment? parentComment = null,
        ContentBody? body = null,
        IEnumerable<MediaReference>? media = null,
        CommentModerationPolicy policy = CommentModerationPolicy.None,
        bool hasPreviouslyApprovedComment = false) =>
        PostComment.Create(
            id ?? PostCommentIdentity.New(),
            UserIdentity.New(),
            postId ?? PostIdentity.New(),
            parentComment,
            body ?? CreateBody(),
            media ?? [],
            policy,
            hasPreviouslyApprovedComment,
            _createdAt);

    private static PostComment ReconstituteAtStatus(
        CommentStatus status,
        PostCommentIdentity? id = null,
        PostIdentity? postId = null,
        IEnumerable<MediaReference>? media = null) =>
        PostComment.Reconstitute(
            id ?? PostCommentIdentity.New(),
            UserIdentity.New(),
            postId ?? PostIdentity.New(),
            null,
            status is CommentStatus.Deleted ? null : CreateBody(),
            status is CommentStatus.Deleted ? [] : media ?? [],
            status,
            _createdAt);

    private static ContentBody CreateBody(
        string source = "Comment body",
        ContentFormat format = ContentFormat.PlainText) =>
        new(source, format);

    private static MediaReference CreateMedia() =>
        new(MediaItemIdentity.New(), "Comment image");
}

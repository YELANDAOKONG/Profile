using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Posts;

public sealed class PostPublicationTests
{
    public static TheoryData<string> PublicationOperations =>
    [
        nameof(Post.Schedule),
        nameof(Post.Unschedule),
        nameof(Post.SubmitForReview),
        nameof(Post.Approve),
        nameof(Post.PublishScheduled),
        nameof(Post.ReturnToDraft),
        nameof(Post.UnpublishToDraft)
    ];

    [Fact]
    public void Schedule_FromDraft_SetsScheduledState()
    {
        var post = PostTestFactory.CreatePost();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);
        var scheduledAt = changedAt.AddDays(1);

        post.Schedule(scheduledAt, changedAt);

        Assert.Equal(PublicationStatus.Scheduled, post.Publication.Status);
        Assert.Equal(scheduledAt, post.Publication.ScheduledAt);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void Unschedule_FromScheduled_ReturnsToDraft()
    {
        var post = PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                PostTestFactory.BaseTime.AddDays(1),
                null,
                null));
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.Unschedule(changedAt);

        Assert.Equal(PublicationStatus.Draft, post.Publication.Status);
        Assert.Null(post.Publication.ScheduledAt);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void SubmitForReview_FromDraft_SetsPendingReview()
    {
        var post = PostTestFactory.CreatePost();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.SubmitForReview(changedAt);

        Assert.Equal(PublicationStatus.PendingReview, post.Publication.Status);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void Approve_FromPendingReview_PublishesAndRecordsTime()
    {
        var post = PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.PendingReview,
                null,
                null,
                null));
        var publishedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.Approve(publishedAt);

        Assert.Equal(PublicationStatus.Published, post.Publication.Status);
        Assert.Equal(publishedAt, post.Publication.FirstPublishedAt);
        Assert.Equal(publishedAt, post.Publication.LastPublishedAt);
        Assert.Equal(publishedAt, post.UpdatedAt);
    }

    [Fact]
    public void PublishScheduled_AtScheduledTime_Publishes()
    {
        var scheduledAt = PostTestFactory.BaseTime.AddDays(1);
        var post = PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                scheduledAt,
                null,
                null));

        post.PublishScheduled(scheduledAt);

        Assert.Equal(PublicationStatus.Published, post.Publication.Status);
        Assert.Null(post.Publication.ScheduledAt);
        Assert.Equal(scheduledAt, post.Publication.FirstPublishedAt);
        Assert.Equal(scheduledAt, post.Publication.LastPublishedAt);
        Assert.Equal(scheduledAt, post.UpdatedAt);
    }

    [Fact]
    public void ReturnToDraft_FromPendingReview_ReturnsToDraft()
    {
        var post = PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.PendingReview,
                null,
                null,
                null));
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.ReturnToDraft(changedAt);

        Assert.Equal(PublicationStatus.Draft, post.Publication.Status);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void UnpublishToDraft_FromPublished_RetainsPublishHistory()
    {
        var post = CreatePublishedPost();
        var changedAt = PostTestFactory.BaseTime.AddMinutes(1);

        post.UnpublishToDraft(changedAt);

        Assert.Equal(PublicationStatus.Draft, post.Publication.Status);
        Assert.Equal(
            PostTestFactory.BaseTime,
            post.Publication.FirstPublishedAt);
        Assert.Equal(
            PostTestFactory.BaseTime,
            post.Publication.LastPublishedAt);
        Assert.Equal(changedAt, post.UpdatedAt);
    }

    [Fact]
    public void Approve_WhenRepublishing_RetainsFirstAndUpdatesLastTime()
    {
        var post = CreatePublishedPost();

        post.UnpublishToDraft(PostTestFactory.BaseTime.AddMinutes(1));
        post.SubmitForReview(PostTestFactory.BaseTime.AddMinutes(2));
        post.Approve(PostTestFactory.BaseTime.AddMinutes(3));

        Assert.Equal(
            PostTestFactory.BaseTime,
            post.Publication.FirstPublishedAt);
        Assert.Equal(
            PostTestFactory.BaseTime.AddMinutes(3),
            post.Publication.LastPublishedAt);
    }

    [Fact]
    public void UnpublishToDraft_FromDraft_ThrowsAndPreservesState()
    {
        var post = PostTestFactory.CreatePost();
        var publication = post.Publication;

        Assert.Throws<InvalidOperationException>(
            () => post.UnpublishToDraft(
                PostTestFactory.BaseTime.AddMinutes(1)));

        Assert.Same(publication, post.Publication);
        Assert.Equal(PostTestFactory.BaseTime, post.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(PublicationOperations))]
    public void PublicationOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var post = CreatePostForOperation(operation);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeOperation(
                post,
                operation,
                post.UpdatedAt.AddTicks(-1)));
    }

    [Theory]
    [MemberData(nameof(PublicationOperations))]
    public void PublicationOperation_WhenDeleted_ThrowsInvalidOperationException(
        string operation)
    {
        var post = CreatePostForOperation(operation, deleted: true);

        Assert.Throws<InvalidOperationException>(
            () => InvokeOperation(post, operation, post.UpdatedAt));
    }

    private static Post CreatePublishedPost() =>
        PostTestFactory.ReconstitutePost(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                PostTestFactory.BaseTime,
                PostTestFactory.BaseTime));

    private static Post CreatePostForOperation(
        string operation,
        bool deleted = false)
    {
        var publication = operation switch
        {
            nameof(Post.Schedule) or nameof(Post.SubmitForReview) =>
                Publication.CreateDraft(),
            nameof(Post.Unschedule) or nameof(Post.PublishScheduled) =>
                Publication.Reconstitute(
                    PublicationStatus.Scheduled,
                    PostTestFactory.BaseTime.AddDays(1),
                    null,
                    null),
            nameof(Post.Approve) or nameof(Post.ReturnToDraft) =>
                Publication.Reconstitute(
                    PublicationStatus.PendingReview,
                    null,
                    null,
                    null),
            nameof(Post.UnpublishToDraft) =>
                Publication.Reconstitute(
                    PublicationStatus.Published,
                    null,
                    PostTestFactory.BaseTime,
                    PostTestFactory.BaseTime),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "Post publication operation is not supported.")
        };

        if (!deleted)
        {
            return PostTestFactory.ReconstitutePost(publication);
        }

        var deletedAt = PostTestFactory.BaseTime.AddMinutes(1);

        return PostTestFactory.ReconstitutePost(
            publication,
            ContentDeletion.Create(deletedAt),
            updatedAt: deletedAt);
    }

    private static void InvokeOperation(
        Post post,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Post.Schedule):
                post.Schedule(changedAt.AddDays(1), changedAt);
                break;
            case nameof(Post.Unschedule):
                post.Unschedule(changedAt);
                break;
            case nameof(Post.SubmitForReview):
                post.SubmitForReview(changedAt);
                break;
            case nameof(Post.Approve):
                post.Approve(changedAt);
                break;
            case nameof(Post.PublishScheduled):
                post.PublishScheduled(changedAt);
                break;
            case nameof(Post.ReturnToDraft):
                post.ReturnToDraft(changedAt);
                break;
            case nameof(Post.UnpublishToDraft):
                post.UnpublishToDraft(changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Post publication operation is not supported.");
        }
    }
}

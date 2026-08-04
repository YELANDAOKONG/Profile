using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Blogs;

public sealed class BlogPublicationTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static TheoryData<string> PublicationOperations =>
    [
        nameof(Blog.Schedule),
        nameof(Blog.Unschedule),
        nameof(Blog.SubmitForReview),
        nameof(Blog.Approve),
        nameof(Blog.PublishScheduled),
        nameof(Blog.ReturnToDraft),
        nameof(Blog.UnpublishToDraft)
    ];

    [Fact]
    public void Schedule_FromDraft_SetsScheduledStateAndUpdateTime()
    {
        var blog = CreateBlog();
        var changedAt = _baseTime.AddMinutes(1);
        var scheduledAt = changedAt.AddDays(1);

        blog.Schedule(scheduledAt, changedAt);

        Assert.Equal(PublicationStatus.Scheduled, blog.Publication.Status);
        Assert.Equal(scheduledAt, blog.Publication.ScheduledAt);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Unschedule_FromScheduled_ReturnsToDraftAndUpdatesTime()
    {
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                _baseTime.AddDays(1),
                null,
                null));
        var changedAt = _baseTime.AddMinutes(1);

        blog.Unschedule(changedAt);

        Assert.Equal(PublicationStatus.Draft, blog.Publication.Status);
        Assert.Null(blog.Publication.ScheduledAt);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void SubmitForReview_FromDraft_SetsPendingReviewAndUpdatesTime()
    {
        var blog = CreateBlog();
        var changedAt = _baseTime.AddMinutes(1);

        blog.SubmitForReview(changedAt);

        Assert.Equal(PublicationStatus.PendingReview, blog.Publication.Status);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Approve_FromPendingReview_PublishesAndRecordsTime()
    {
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.PendingReview,
                null,
                null,
                null));
        var publishedAt = _baseTime.AddMinutes(1);

        blog.Approve(publishedAt);

        Assert.Equal(PublicationStatus.Published, blog.Publication.Status);
        Assert.Equal(publishedAt, blog.Publication.FirstPublishedAt);
        Assert.Equal(publishedAt, blog.Publication.LastPublishedAt);
        Assert.Equal(publishedAt, blog.UpdatedAt);
    }

    [Fact]
    public void PublishScheduled_AtScheduledTime_PublishesAndUpdatesTime()
    {
        var scheduledAt = _baseTime.AddDays(1);
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                scheduledAt,
                null,
                null));

        blog.PublishScheduled(scheduledAt);

        Assert.Equal(PublicationStatus.Published, blog.Publication.Status);
        Assert.Null(blog.Publication.ScheduledAt);
        Assert.Equal(scheduledAt, blog.Publication.FirstPublishedAt);
        Assert.Equal(scheduledAt, blog.Publication.LastPublishedAt);
        Assert.Equal(scheduledAt, blog.UpdatedAt);
    }

    [Fact]
    public void ReturnToDraft_FromPendingReview_ReturnsToDraftAndUpdatesTime()
    {
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.PendingReview,
                null,
                null,
                null));
        var changedAt = _baseTime.AddMinutes(1);

        blog.ReturnToDraft(changedAt);

        Assert.Equal(PublicationStatus.Draft, blog.Publication.Status);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void UnpublishToDraft_FromPublished_RetainsHistoryAndUpdatesTime()
    {
        var firstPublishedAt = _baseTime;
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                firstPublishedAt,
                firstPublishedAt));
        var changedAt = _baseTime.AddMinutes(1);

        blog.UnpublishToDraft(changedAt);

        Assert.Equal(PublicationStatus.Draft, blog.Publication.Status);
        Assert.Equal(firstPublishedAt, blog.Publication.FirstPublishedAt);
        Assert.Equal(firstPublishedAt, blog.Publication.LastPublishedAt);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Approve_WhenRepublishing_RetainsFirstTimeAndUpdatesLastTime()
    {
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                _baseTime,
                _baseTime));

        blog.UnpublishToDraft(_baseTime.AddMinutes(1));
        blog.SubmitForReview(_baseTime.AddMinutes(2));
        blog.Approve(_baseTime.AddMinutes(3));

        Assert.Equal(_baseTime, blog.Publication.FirstPublishedAt);
        Assert.Equal(_baseTime.AddMinutes(3), blog.Publication.LastPublishedAt);
        Assert.Equal(_baseTime.AddMinutes(3), blog.UpdatedAt);
    }

    [Fact]
    public void Schedule_WithInvalidScheduledTime_DoesNotChangeBlog()
    {
        var blog = CreateBlog();
        var publication = blog.Publication;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => blog.Schedule(_baseTime, _baseTime));

        Assert.Same(publication, blog.Publication);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void UnpublishToDraft_FromDraft_DoesNotChangeBlog()
    {
        var blog = CreateBlog();
        var publication = blog.Publication;

        Assert.Throws<InvalidOperationException>(
            () => blog.UnpublishToDraft(_baseTime.AddMinutes(1)));

        Assert.Same(publication, blog.Publication);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void PublishScheduled_BeforeScheduledTime_DoesNotChangeBlog()
    {
        var scheduledAt = _baseTime.AddDays(1);
        var blog = ReconstituteBlog(
            Publication.Reconstitute(
                PublicationStatus.Scheduled,
                scheduledAt,
                null,
                null));
        var publication = blog.Publication;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => blog.PublishScheduled(scheduledAt.AddTicks(-1)));

        Assert.Same(publication, blog.Publication);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(PublicationOperations))]
    public void PublicationOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var blog = CreateBlogForOperation(operation);
        var changedAt = blog.UpdatedAt.AddTicks(-1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeOperation(blog, operation, changedAt));

        var expectedParameter = operation is nameof(Blog.Approve)
            or nameof(Blog.PublishScheduled)
            ? "publishedAt"
            : "changedAt";
        Assert.Equal(expectedParameter, exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(PublicationOperations))]
    public void PublicationOperation_WhenBlogIsDeleted_ThrowsInvalidOperationException(
        string operation)
    {
        var blog = CreateBlogForOperation(operation, deleted: true);
        var publication = blog.Publication;

        Assert.Throws<InvalidOperationException>(
            () => InvokeOperation(blog, operation, blog.UpdatedAt));

        Assert.Same(publication, blog.Publication);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    private static Blog CreateBlog() =>
        ReconstituteBlog(Publication.CreateDraft());

    private static Blog CreateBlogForOperation(
        string operation,
        bool deleted = false)
    {
        var publication = operation switch
        {
            nameof(Blog.Schedule) or nameof(Blog.SubmitForReview) =>
                Publication.CreateDraft(),
            nameof(Blog.Unschedule) or nameof(Blog.PublishScheduled) =>
                Publication.Reconstitute(
                    PublicationStatus.Scheduled,
                    _baseTime.AddDays(1),
                    null,
                    null),
            nameof(Blog.Approve) or nameof(Blog.ReturnToDraft) =>
                Publication.Reconstitute(
                    PublicationStatus.PendingReview,
                    null,
                    null,
                    null),
            nameof(Blog.UnpublishToDraft) =>
                Publication.Reconstitute(
                    PublicationStatus.Published,
                    null,
                    _baseTime,
                    _baseTime),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "Publication operation is not supported.")
        };

        return ReconstituteBlog(
            publication,
            deleted ? ContentDeletion.Create(_baseTime) : null);
    }

    private static Blog ReconstituteBlog(
        Publication publication,
        ContentDeletion? deletion = null) =>
        Blog.Reconstitute(
            BlogIdentity.New(),
            UserIdentity.New(),
            new BlogSlug("000000001"),
            "Blog title",
            new ContentBlockCollection([]),
            null,
            null,
            ContentVisibility.Public,
            null,
            [],
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            pinned: false,
            featured: false,
            null,
            null,
            null,
            [],
            publication,
            deletion,
            _baseTime,
            _baseTime);

    private static void InvokeOperation(
        Blog blog,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Blog.Schedule):
                blog.Schedule(changedAt.AddDays(1), changedAt);
                break;
            case nameof(Blog.Unschedule):
                blog.Unschedule(changedAt);
                break;
            case nameof(Blog.SubmitForReview):
                blog.SubmitForReview(changedAt);
                break;
            case nameof(Blog.Approve):
                blog.Approve(changedAt);
                break;
            case nameof(Blog.PublishScheduled):
                blog.PublishScheduled(changedAt);
                break;
            case nameof(Blog.ReturnToDraft):
                blog.ReturnToDraft(changedAt);
                break;
            case nameof(Blog.UnpublishToDraft):
                blog.UnpublishToDraft(changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Publication operation is not supported.");
        }
    }
}

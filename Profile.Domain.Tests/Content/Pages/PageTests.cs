using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Pages;
using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Pages;

public sealed class PageTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesValuesAndStartsAsDraft()
    {
        var id = PageIdentity.New();
        var authorId = UserIdentity.New();
        var routeIdentifier = new PageRouteIdentifier("About-Us");
        var blocks = CreateBlocks("Body");
        var featuredMedia = new MediaReference(
            MediaItemIdentity.New(),
            "Featured image");

        var page = Page.Create(
            id,
            authorId,
            routeIdentifier,
            "About us",
            blocks,
            ContentVisibility.Followers,
            commentsAllowed: false,
            CommenterPolicy.AuthorOnly,
            "SEO title",
            "SEO description",
            featuredMedia,
            _baseTime);

        Assert.Equal(id, page.Id);
        Assert.Equal(authorId, page.AuthorId);
        Assert.Equal(routeIdentifier, page.RouteIdentifier);
        Assert.Equal("About us", page.Title);
        Assert.Same(blocks, page.Blocks);
        Assert.Equal(ContentVisibility.Followers, page.Visibility);
        Assert.False(page.CommentsAllowed);
        Assert.Equal(CommenterPolicy.AuthorOnly, page.CommenterPolicy);
        Assert.Equal("SEO title", page.SeoTitle);
        Assert.Equal("SEO description", page.SeoDescription);
        Assert.Equal(featuredMedia, page.FeaturedMedia);
        Assert.Equal(PagePublicationStatus.Draft, page.Publication.Status);
        Assert.Null(page.Deletion);
        Assert.Equal(_baseTime, page.CreatedAt);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void Create_WithEmptyBlocks_AllowsDraft()
    {
        var page = CreatePage(blocks: new ContentBlockCollection([]));

        Assert.Empty(page.Blocks.Blocks);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyOrWhitespaceTitle_ThrowsArgumentException(
        string title)
    {
        Assert.Throws<ArgumentException>(
            () => CreatePage(title: title));
    }

    [Fact]
    public void Create_WithTitleAtMaximumLength_AcceptsValue()
    {
        var title = new string('x', Page.MaximumTitleLength);

        var page = CreatePage(title: title);

        Assert.Equal(title, page.Title);
    }

    [Fact]
    public void Create_WithTitleAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var title = new string('x', Page.MaximumTitleLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePage(title: title));
    }

    [Fact]
    public void Create_WithSeoValuesAtMaximumLengths_AcceptsValues()
    {
        var seoTitle = new string('x', Page.MaximumSeoTitleLength);
        var seoDescription = new string(
            'x',
            Page.MaximumSeoDescriptionLength);

        var page = CreatePage(
            seoTitle: seoTitle,
            seoDescription: seoDescription);

        Assert.Equal(seoTitle, page.SeoTitle);
        Assert.Equal(seoDescription, page.SeoDescription);
    }

    [Fact]
    public void Create_WithSeoTitleAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePage(
                seoTitle: new string(
                    'x',
                    Page.MaximumSeoTitleLength + 1)));
    }

    [Fact]
    public void Create_WithSeoDescriptionAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePage(
                seoDescription: new string(
                    'x',
                    Page.MaximumSeoDescriptionLength + 1)));
    }

    [Fact]
    public void Create_WithUndefinedVisibility_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePage(
                visibility: (ContentVisibility)int.MaxValue));
    }

    [Fact]
    public void Create_WithUndefinedCommenterPolicy_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreatePage(
                commenterPolicy: (CommenterPolicy)int.MaxValue));
    }

    [Fact]
    public void Reconstitute_WithPublishedDeletionState_PreservesValues()
    {
        var publishedAt = _baseTime.AddMinutes(1);
        var deletedAt = _baseTime.AddMinutes(2);
        var publication = PagePublication.Reconstitute(
            PagePublicationStatus.Published,
            publishedAt,
            publishedAt);
        var deletion = ContentDeletion.Create(deletedAt);

        var page = ReconstitutePage(
            publication,
            deletion,
            deletedAt);

        Assert.Same(publication, page.Publication);
        Assert.Same(deletion, page.Deletion);
        Assert.Equal(deletedAt, page.UpdatedAt);
    }

    [Fact]
    public void Reconstitute_WithUpdateBeforeCreation_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstitutePage(
                PagePublication.CreateDraft(),
                null,
                _baseTime.AddTicks(-1)));
    }

    [Fact]
    public void Reconstitute_WithPublishTimeBeforeCreation_ThrowsArgumentOutOfRangeException()
    {
        var publishedAt = _baseTime.AddTicks(-1);
        var publication = PagePublication.Reconstitute(
            PagePublicationStatus.Published,
            publishedAt,
            publishedAt);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstitutePage(
                publication,
                null,
                _baseTime));
    }

    [Fact]
    public void Reconstitute_WithDeletionAfterUpdate_ThrowsArgumentOutOfRangeException()
    {
        var deletion = ContentDeletion.Create(_baseTime.AddMinutes(1));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstitutePage(
                PagePublication.CreateDraft(),
                deletion,
                _baseTime));
    }

    private static Page CreatePage(
        string title = "Page title",
        ContentBlockCollection? blocks = null,
        ContentVisibility visibility = ContentVisibility.Public,
        CommenterPolicy commenterPolicy = CommenterPolicy.AllReaders,
        string? seoTitle = null,
        string? seoDescription = null) =>
        Page.Create(
            PageIdentity.New(),
            UserIdentity.New(),
            new PageRouteIdentifier("about"),
            title,
            blocks ?? new ContentBlockCollection([]),
            visibility,
            commentsAllowed: true,
            commenterPolicy,
            seoTitle,
            seoDescription,
            null,
            _baseTime);

    private static Page ReconstitutePage(
        PagePublication publication,
        ContentDeletion? deletion,
        DateTimeOffset updatedAt) =>
        Page.Reconstitute(
            PageIdentity.New(),
            UserIdentity.New(),
            new PageRouteIdentifier("about"),
            "Page title",
            new ContentBlockCollection([]),
            ContentVisibility.Public,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            null,
            null,
            null,
            publication,
            deletion,
            _baseTime,
            updatedAt);

    private static ContentBlockCollection CreateBlocks(string source) =>
        new(
        [
            new TextBlock(new ContentBody(source, ContentFormat.Markdown))
        ]);
}

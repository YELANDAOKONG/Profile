using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Pages;
using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Pages;

public sealed class PageEditingTests
{
    private const int _customReservationPeriodDays = 45;

    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ChangeRouteIdentifier_WithDifferentValue_ReturnsReservation()
    {
        var page = CreatePage();
        var changedAt = _baseTime.AddMinutes(1);
        var routeIdentifier = new PageRouteIdentifier("Contact");

        var reservation = page.ChangeRouteIdentifier(
            routeIdentifier,
            changedAt);

        Assert.Equal(routeIdentifier, page.RouteIdentifier);
        Assert.Equal(changedAt, page.UpdatedAt);
        Assert.NotNull(reservation);
        Assert.Equal(page.Id, reservation.PageId);
        Assert.Equal(page.AuthorId, reservation.OwnerId);
        Assert.Equal(
            new PageRouteIdentifier("About"),
            reservation.RouteIdentifier);
        Assert.Equal(changedAt, reservation.ReservedAt);
        Assert.Equal(
            changedAt.AddDays(PageRouteReservation.DefaultReservationPeriodDays),
            reservation.ReleasesAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithCustomPeriod_FixesReleaseTime()
    {
        var page = CreatePage();
        var changedAt = _baseTime.AddMinutes(1);
        var period = TimeSpan.FromDays(_customReservationPeriodDays);

        var reservation = page.ChangeRouteIdentifier(
            new PageRouteIdentifier("contact"),
            changedAt,
            period);

        Assert.Equal(changedAt.Add(period), reservation?.ReleasesAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithCasingOnlyChange_DoesNotReserveOldCasing()
    {
        var page = CreatePage();
        var changedAt = _baseTime.AddMinutes(1);

        var reservation = page.ChangeRouteIdentifier(
            new PageRouteIdentifier("ABOUT"),
            changedAt);

        Assert.Null(reservation);
        Assert.Equal("ABOUT", page.RouteIdentifier.Value);
        Assert.Equal("about", page.RouteIdentifier.NormalizedValue);
        Assert.Equal(changedAt, page.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithExactValue_DoesNotUpdatePage()
    {
        var page = CreatePage();

        var reservation = page.ChangeRouteIdentifier(
            new PageRouteIdentifier("About"),
            _baseTime.AddMinutes(1));

        Assert.Null(reservation);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithNonPositivePeriod_DoesNotChangePage()
    {
        var page = CreatePage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => page.ChangeRouteIdentifier(
                new PageRouteIdentifier("contact"),
                _baseTime.AddMinutes(1),
                TimeSpan.Zero));

        Assert.Equal(new PageRouteIdentifier("About"), page.RouteIdentifier);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_WithValidValues_ReplacesContent()
    {
        var page = CreatePage();
        var blocks = CreateBlocks("Changed");
        var featuredMedia = new MediaReference(
            MediaItemIdentity.New(),
            "Featured image");
        var changedAt = _baseTime.AddMinutes(1);

        page.UpdateContent(
            "Changed title",
            blocks,
            featuredMedia,
            changedAt);

        Assert.Equal("Changed title", page.Title);
        Assert.Same(blocks, page.Blocks);
        Assert.Equal(featuredMedia, page.FeaturedMedia);
        Assert.Equal(changedAt, page.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_WithEmptyBlocks_AllowsValue()
    {
        var page = CreatePage(blocks: CreateBlocks("Original"));
        var blocks = new ContentBlockCollection([]);

        page.UpdateContent(
            "Changed title",
            blocks,
            null,
            _baseTime.AddMinutes(1));

        Assert.Empty(page.Blocks.Blocks);
    }

    [Fact]
    public void UpdateContent_WithInvalidTitle_DoesNotChangePage()
    {
        var originalBlocks = CreateBlocks("Original");
        var page = CreatePage(blocks: originalBlocks);

        Assert.Throws<ArgumentException>(
            () => page.UpdateContent(
                " ",
                CreateBlocks("Changed"),
                null,
                _baseTime.AddMinutes(1)));

        Assert.Equal("Page title", page.Title);
        Assert.Same(originalBlocks, page.Blocks);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void ConfigurationChanges_ReplaceIndependentSettings()
    {
        var page = CreatePage();

        page.ChangeVisibility(
            ContentVisibility.MutualFollowers,
            _baseTime.AddMinutes(1));
        page.ChangeDiscussion(
            commentsAllowed: false,
            CommenterPolicy.AuthorOnly,
            _baseTime.AddMinutes(2));
        page.UpdateSearchMetadata(
            "SEO title",
            "SEO description",
            _baseTime.AddMinutes(3));

        Assert.Equal(ContentVisibility.MutualFollowers, page.Visibility);
        Assert.False(page.CommentsAllowed);
        Assert.Equal(CommenterPolicy.AuthorOnly, page.CommenterPolicy);
        Assert.Equal("SEO title", page.SeoTitle);
        Assert.Equal("SEO description", page.SeoDescription);
        Assert.Equal(_baseTime.AddMinutes(3), page.UpdatedAt);
    }

    [Fact]
    public void UpdateSearchMetadata_WithInvalidValue_DoesNotChangePage()
    {
        var page = CreatePage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => page.UpdateSearchMetadata(
                new string('x', Page.MaximumSeoTitleLength + 1),
                "Changed description",
                _baseTime.AddMinutes(1)));

        Assert.Null(page.SeoTitle);
        Assert.Null(page.SeoDescription);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    [Fact]
    public void Edit_WhenPageIsDeleted_ThrowsInvalidOperationException()
    {
        var page = CreatePage();
        page.Delete(_baseTime.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            () => page.ChangeRouteIdentifier(
                new PageRouteIdentifier("contact"),
                _baseTime.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(
            () => page.UpdateContent(
                "Changed",
                new ContentBlockCollection([]),
                null,
                _baseTime.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(
            () => page.ChangeVisibility(
                ContentVisibility.Private,
                _baseTime.AddMinutes(2)));
    }

    [Fact]
    public void Edit_WithEarlierTime_ThrowsArgumentOutOfRangeException()
    {
        var page = CreatePage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => page.ChangeVisibility(
                ContentVisibility.Private,
                _baseTime.AddTicks(-1)));

        Assert.Equal(ContentVisibility.Public, page.Visibility);
        Assert.Equal(_baseTime, page.UpdatedAt);
    }

    private static Page CreatePage(
        ContentBlockCollection? blocks = null) =>
        Page.Create(
            PageIdentity.New(),
            UserIdentity.New(),
            new PageRouteIdentifier("About"),
            "Page title",
            blocks ?? new ContentBlockCollection([]),
            ContentVisibility.Public,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            null,
            null,
            null,
            _baseTime);

    private static ContentBlockCollection CreateBlocks(string source) =>
        new(
        [
            new TextBlock(new ContentBody(source, ContentFormat.Markdown))
        ]);
}

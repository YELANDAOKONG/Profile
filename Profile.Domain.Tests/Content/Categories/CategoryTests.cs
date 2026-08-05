using Profile.Domain.Content.Categories;
using Profile.Domain.Content.Categories.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Categories;

public sealed class CategoryTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesValues()
    {
        var id = CategoryIdentity.New();
        var ownerId = UserIdentity.New();
        var name = new TaxonomyName("Engineering");
        var routeIdentifier = new TaxonomyRouteIdentifier("Engineering");
        var coverMedia = new MediaReference(MediaItemIdentity.New(), "Cover");

        var category = Category.Create(
            id,
            ownerId,
            name,
            routeIdentifier,
            "Engineering articles",
            coverMedia,
            "Engineering",
            "Articles about engineering",
            3,
            _createdAt);

        Assert.Equal(id, category.Id);
        Assert.Equal(ownerId, category.OwnerId);
        Assert.Same(name, category.Name);
        Assert.Same(routeIdentifier, category.RouteIdentifier);
        Assert.Equal("Engineering articles", category.Description);
        Assert.Same(coverMedia, category.CoverMedia);
        Assert.Equal("Engineering", category.SeoTitle);
        Assert.Equal("Articles about engineering", category.SeoDescription);
        Assert.Equal(3, category.SortOrder);
        Assert.Equal(_createdAt, category.CreatedAt);
        Assert.Equal(_createdAt, category.UpdatedAt);
    }

    [Fact]
    public void Create_WithNullPresentationValues_AcceptsState()
    {
        var category = CreateCategory();

        Assert.Null(category.Description);
        Assert.Null(category.CoverMedia);
        Assert.Null(category.SeoTitle);
        Assert.Null(category.SeoDescription);
    }

    [Fact]
    public void Rename_WithValidValue_ReplacesName()
    {
        var category = CreateCategory();
        var name = new TaxonomyName("Architecture");
        var changedAt = _createdAt.AddMinutes(1);

        category.Rename(name, changedAt);

        Assert.Same(name, category.Name);
        Assert.Equal(changedAt, category.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithSubstantiveChange_ReturnsReservationAndReplacesRoute()
    {
        var category = CreateCategory();
        var previousRoute = category.RouteIdentifier;
        var changedAt = _createdAt.AddMinutes(1);
        var replacement = new TaxonomyRouteIdentifier("architecture");

        var reservation = category.ChangeRouteIdentifier(replacement, changedAt);

        Assert.NotNull(reservation);
        Assert.Equal(category.Id, reservation.CategoryId);
        Assert.Equal(category.OwnerId, reservation.OwnerId);
        Assert.Same(previousRoute, reservation.RouteIdentifier);
        Assert.Equal(changedAt, reservation.ReservedAt);
        Assert.Equal(
            changedAt.Add(CategoryRouteReservation.DefaultReservationPeriod),
            reservation.ReleasesAt);
        Assert.Same(replacement, category.RouteIdentifier);
        Assert.Equal(changedAt, category.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithCasingOnlyChange_DoesNotCreateReservation()
    {
        var category = CreateCategory(
            routeIdentifier: new TaxonomyRouteIdentifier("Engineering"));
        var replacement = new TaxonomyRouteIdentifier("engineering");
        var changedAt = _createdAt.AddMinutes(1);

        var reservation = category.ChangeRouteIdentifier(replacement, changedAt);

        Assert.Null(reservation);
        Assert.Same(replacement, category.RouteIdentifier);
        Assert.Equal(changedAt, category.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithExactSameValue_DoesNotChangeUpdatedTime()
    {
        var category = CreateCategory();

        var reservation = category.ChangeRouteIdentifier(
            new TaxonomyRouteIdentifier("engineering"),
            _createdAt.AddMinutes(1));

        Assert.Null(reservation);
        Assert.Equal(_createdAt, category.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithNonPositivePeriod_ThrowsAndPreservesState()
    {
        var category = CreateCategory();
        var routeIdentifier = category.RouteIdentifier;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.ChangeRouteIdentifier(
                new TaxonomyRouteIdentifier("architecture"),
                _createdAt.AddMinutes(1),
                TimeSpan.Zero));

        Assert.Same(routeIdentifier, category.RouteIdentifier);
        Assert.Equal(_createdAt, category.UpdatedAt);
    }

    [Fact]
    public void UpdatePresentation_WithValidValues_ReplacesPresentation()
    {
        var category = CreateCategory();
        var coverMedia = new MediaReference(MediaItemIdentity.New(), "Cover");
        var changedAt = _createdAt.AddMinutes(1);

        category.UpdatePresentation(
            "Architecture articles",
            coverMedia,
            "Architecture",
            "Articles about architecture",
            changedAt);

        Assert.Equal("Architecture articles", category.Description);
        Assert.Same(coverMedia, category.CoverMedia);
        Assert.Equal("Architecture", category.SeoTitle);
        Assert.Equal("Articles about architecture", category.SeoDescription);
        Assert.Equal(changedAt, category.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" Description")]
    [InlineData("Description ")]
    public void Create_WithInvalidDescription_ThrowsArgumentException(
        string description)
    {
        Assert.Throws<ArgumentException>(
            () => CreateCategory(description: description));
    }

    [Fact]
    public void Create_WithDescriptionAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var description = new string('a', Category.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateCategory(description: description));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" SEO")]
    [InlineData("SEO ")]
    public void Create_WithInvalidSeoTitle_ThrowsArgumentException(
        string seoTitle)
    {
        Assert.Throws<ArgumentException>(
            () => CreateCategory(seoTitle: seoTitle));
    }

    [Fact]
    public void Create_WithSeoDescriptionAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var seoDescription = new string(
            'a',
            Category.MaximumSeoDescriptionLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateCategory(seoDescription: seoDescription));
    }

    [Fact]
    public void Reorder_WithNegativeValue_ThrowsAndPreservesState()
    {
        var category = CreateCategory();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.Reorder(-1, _createdAt.AddMinutes(1)));

        Assert.Equal(0, category.SortOrder);
        Assert.Equal(_createdAt, category.UpdatedAt);
    }

    [Fact]
    public void Mutations_WithEarlierTime_ThrowArgumentOutOfRangeException()
    {
        var category = CreateCategory();
        var earlier = _createdAt.AddTicks(-1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.Rename(new TaxonomyName("Architecture"), earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.ChangeRouteIdentifier(
                new TaxonomyRouteIdentifier("architecture"),
                earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.UpdatePresentation(null, null, null, null, earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.Reorder(1, earlier));
    }

    [Fact]
    public void Reconstitute_WithUpdatedTimeBeforeCreatedTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Category.Reconstitute(
                CategoryIdentity.New(),
                UserIdentity.New(),
                new TaxonomyName("Engineering"),
                new TaxonomyRouteIdentifier("engineering"),
                null,
                null,
                null,
                null,
                0,
                _createdAt,
                _createdAt.AddTicks(-1)));
    }

    private static Category CreateCategory(
        TaxonomyRouteIdentifier? routeIdentifier = null,
        string? description = null,
        string? seoTitle = null,
        string? seoDescription = null) =>
        Category.Create(
            CategoryIdentity.New(),
            UserIdentity.New(),
            new TaxonomyName("Engineering"),
            routeIdentifier ?? new TaxonomyRouteIdentifier("engineering"),
            description,
            null,
            seoTitle,
            seoDescription,
            0,
            _createdAt);
}

using Profile.Domain.Content.Tags;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Tags;

public sealed class MomentTagTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesValues()
    {
        var id = MomentTagIdentity.New();
        var ownerId = UserIdentity.New();
        var name = new TaxonomyName("DotNet");
        var routeIdentifier = new TaxonomyRouteIdentifier("DotNet");
        var coverMedia = new MediaReference(MediaItemIdentity.New(), "Cover");

        var tag = MomentTag.Create(
            id,
            ownerId,
            name,
            routeIdentifier,
            "DotNet content",
            coverMedia,
            "DotNet",
            "Content about DotNet",
            3,
            _createdAt);

        Assert.Equal(id, tag.Id);
        Assert.Equal(ownerId, tag.OwnerId);
        Assert.Same(name, tag.Name);
        Assert.Same(routeIdentifier, tag.RouteIdentifier);
        Assert.Equal("DotNet content", tag.Description);
        Assert.Same(coverMedia, tag.CoverMedia);
        Assert.Equal("DotNet", tag.SeoTitle);
        Assert.Equal("Content about DotNet", tag.SeoDescription);
        Assert.Equal(3, tag.SortOrder);
        Assert.Equal(_createdAt, tag.CreatedAt);
        Assert.Equal(_createdAt, tag.UpdatedAt);
    }

    [Fact]
    public void Rename_WithValidValue_ReplacesName()
    {
        var tag = CreateTag();
        var name = new TaxonomyName("AspNet");
        var changedAt = _createdAt.AddMinutes(1);

        tag.Rename(name, changedAt);

        Assert.Same(name, tag.Name);
        Assert.Equal(changedAt, tag.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithSubstantiveChange_ReturnsReservationAndReplacesRoute()
    {
        var tag = CreateTag();
        var previousRoute = tag.RouteIdentifier;
        var changedAt = _createdAt.AddMinutes(1);
        var replacement = new TaxonomyRouteIdentifier("aspnet");

        var reservation = tag.ChangeRouteIdentifier(replacement, changedAt);

        Assert.NotNull(reservation);
        Assert.Equal(tag.Id, reservation.TagId);
        Assert.Equal(tag.OwnerId, reservation.OwnerId);
        Assert.Same(previousRoute, reservation.RouteIdentifier);
        Assert.Equal(changedAt, reservation.ReservedAt);
        Assert.Equal(
            changedAt.Add(MomentTagRouteReservation.DefaultReservationPeriod),
            reservation.ReleasesAt);
        Assert.Same(replacement, tag.RouteIdentifier);
        Assert.Equal(changedAt, tag.UpdatedAt);
    }

    [Fact]
    public void ChangeRouteIdentifier_WithCasingOnlyChange_DoesNotCreateReservation()
    {
        var tag = CreateTag(
            routeIdentifier: new TaxonomyRouteIdentifier("DotNet"));
        var replacement = new TaxonomyRouteIdentifier("dotnet");
        var changedAt = _createdAt.AddMinutes(1);

        var reservation = tag.ChangeRouteIdentifier(replacement, changedAt);

        Assert.Null(reservation);
        Assert.Same(replacement, tag.RouteIdentifier);
        Assert.Equal(changedAt, tag.UpdatedAt);
    }

    [Fact]
    public void UpdatePresentation_WithValidValues_ReplacesPresentation()
    {
        var tag = CreateTag();
        var coverMedia = new MediaReference(MediaItemIdentity.New(), "Cover");
        var changedAt = _createdAt.AddMinutes(1);

        tag.UpdatePresentation(
            "AspNet content",
            coverMedia,
            "AspNet",
            "Content about AspNet",
            changedAt);

        Assert.Equal("AspNet content", tag.Description);
        Assert.Same(coverMedia, tag.CoverMedia);
        Assert.Equal("AspNet", tag.SeoTitle);
        Assert.Equal("Content about AspNet", tag.SeoDescription);
        Assert.Equal(changedAt, tag.UpdatedAt);
    }

    [Fact]
    public void Reorder_WithValidValue_ReplacesSortOrder()
    {
        var tag = CreateTag();
        var changedAt = _createdAt.AddMinutes(1);

        tag.Reorder(7, changedAt);

        Assert.Equal(7, tag.SortOrder);
        Assert.Equal(changedAt, tag.UpdatedAt);
    }

    [Fact]
    public void Create_WithInvalidDescription_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateTag(description: " Description"));
    }

    [Fact]
    public void Create_WithDescriptionAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var description = new string('a', MomentTag.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTag(description: description));
    }

    [Fact]
    public void Create_WithSeoTitleAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var seoTitle = new string('a', MomentTag.MaximumSeoTitleLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTag(seoTitle: seoTitle));
    }

    [Fact]
    public void Create_WithNegativeSortOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MomentTag.Create(
                MomentTagIdentity.New(),
                UserIdentity.New(),
                new TaxonomyName("DotNet"),
                new TaxonomyRouteIdentifier("dotnet"),
                null,
                null,
                null,
                null,
                -1,
                _createdAt));
    }

    [Fact]
    public void Mutations_WithEarlierTime_ThrowArgumentOutOfRangeException()
    {
        var tag = CreateTag();
        var earlier = _createdAt.AddTicks(-1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => tag.Rename(new TaxonomyName("AspNet"), earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => tag.ChangeRouteIdentifier(
                new TaxonomyRouteIdentifier("aspnet"),
                earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => tag.UpdatePresentation(null, null, null, null, earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => tag.Reorder(1, earlier));
    }

    [Fact]
    public void Reconstitute_WithUpdatedTimeBeforeCreatedTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MomentTag.Reconstitute(
                MomentTagIdentity.New(),
                UserIdentity.New(),
                new TaxonomyName("DotNet"),
                new TaxonomyRouteIdentifier("dotnet"),
                null,
                null,
                null,
                null,
                0,
                _createdAt,
                _createdAt.AddTicks(-1)));
    }

    private static MomentTag CreateTag(
        TaxonomyRouteIdentifier? routeIdentifier = null,
        string? description = null,
        string? seoTitle = null) =>
        MomentTag.Create(
            MomentTagIdentity.New(),
            UserIdentity.New(),
            new TaxonomyName("DotNet"),
            routeIdentifier ?? new TaxonomyRouteIdentifier("dotnet"),
            description,
            null,
            seoTitle,
            null,
            0,
            _createdAt);
}

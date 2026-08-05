using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Taxonomy;
using Profile.Domain.Content.Taxonomy.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Tags;

public sealed class MomentTag
{
    public const int MaximumCountPerAccount = 8_192;
    public const int MaximumDescriptionLength = 1_024;
    public const int MaximumSeoTitleLength = 128;
    public const int MaximumSeoDescriptionLength = 512;

    private MomentTag(
        MomentTagIdentity id,
        UserIdentity ownerId,
        TaxonomyName name,
        TaxonomyRouteIdentifier routeIdentifier,
        string? description,
        MediaReference? coverMedia,
        string? seoTitle,
        string? seoDescription,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(routeIdentifier);

        ValidatePresentation(description, seoTitle, seoDescription);
        TaxonomyDomainRules.ValidateSortOrder(sortOrder);
        TaxonomyDomainRules.ValidateTimestamps(createdAt, updatedAt);

        Id = id;
        OwnerId = ownerId;
        Name = name;
        RouteIdentifier = routeIdentifier;
        Description = description;
        CoverMedia = coverMedia;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public MomentTagIdentity Id { get; }

    public UserIdentity OwnerId { get; }

    public TaxonomyName Name { get; private set; }

    public TaxonomyRouteIdentifier RouteIdentifier { get; private set; }

    public string? Description { get; private set; }

    public MediaReference? CoverMedia { get; private set; }

    public string? SeoTitle { get; private set; }

    public string? SeoDescription { get; private set; }

    public long SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static MomentTag Create(
        MomentTagIdentity id,
        UserIdentity ownerId,
        TaxonomyName name,
        TaxonomyRouteIdentifier routeIdentifier,
        string? description,
        MediaReference? coverMedia,
        string? seoTitle,
        string? seoDescription,
        long sortOrder,
        DateTimeOffset createdAt) =>
        new(
            id,
            ownerId,
            name,
            routeIdentifier,
            description,
            coverMedia,
            seoTitle,
            seoDescription,
            sortOrder,
            createdAt,
            createdAt);

    public static MomentTag Reconstitute(
        MomentTagIdentity id,
        UserIdentity ownerId,
        TaxonomyName name,
        TaxonomyRouteIdentifier routeIdentifier,
        string? description,
        MediaReference? coverMedia,
        string? seoTitle,
        string? seoDescription,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(
            id,
            ownerId,
            name,
            routeIdentifier,
            description,
            coverMedia,
            seoTitle,
            seoDescription,
            sortOrder,
            createdAt,
            updatedAt);

    public void Rename(TaxonomyName name, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(name);

        TaxonomyDomainRules.EnsureMutationTime(changedAt, UpdatedAt);

        Name = name;
        UpdatedAt = changedAt;
    }

    public MomentTagRouteReservation? ChangeRouteIdentifier(
        TaxonomyRouteIdentifier routeIdentifier,
        DateTimeOffset changedAt) =>
        ChangeRouteIdentifier(
            routeIdentifier,
            changedAt,
            MomentTagRouteReservation.DefaultReservationPeriod);

    public MomentTagRouteReservation? ChangeRouteIdentifier(
        TaxonomyRouteIdentifier routeIdentifier,
        DateTimeOffset changedAt,
        TimeSpan reservationPeriod)
    {
        ArgumentNullException.ThrowIfNull(routeIdentifier);

        TaxonomyDomainRules.EnsureMutationTime(changedAt, UpdatedAt);

        if (reservationPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reservationPeriod),
                reservationPeriod,
                "Moment tag route reservation period must be greater than zero.");
        }

        if (string.Equals(
            RouteIdentifier.Value,
            routeIdentifier.Value,
            StringComparison.Ordinal))
        {
            return null;
        }

        if (RouteIdentifier.Equals(routeIdentifier))
        {
            RouteIdentifier = routeIdentifier;
            UpdatedAt = changedAt;

            return null;
        }

        var reservation = MomentTagRouteReservation.Create(
            Id,
            OwnerId,
            RouteIdentifier,
            changedAt,
            reservationPeriod);

        RouteIdentifier = routeIdentifier;
        UpdatedAt = changedAt;

        return reservation;
    }

    public void UpdatePresentation(
        string? description,
        MediaReference? coverMedia,
        string? seoTitle,
        string? seoDescription,
        DateTimeOffset changedAt)
    {
        TaxonomyDomainRules.EnsureMutationTime(changedAt, UpdatedAt);
        ValidatePresentation(description, seoTitle, seoDescription);

        Description = description;
        CoverMedia = coverMedia;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        UpdatedAt = changedAt;
    }

    public void Reorder(long sortOrder, DateTimeOffset changedAt)
    {
        TaxonomyDomainRules.EnsureMutationTime(changedAt, UpdatedAt);
        TaxonomyDomainRules.ValidateSortOrder(sortOrder);

        SortOrder = sortOrder;
        UpdatedAt = changedAt;
    }

    private static void ValidatePresentation(
        string? description,
        string? seoTitle,
        string? seoDescription)
    {
        TaxonomyDomainRules.ValidateOptionalText(
            description,
            MaximumDescriptionLength,
            nameof(description),
            "Moment tag description");
        TaxonomyDomainRules.ValidateOptionalText(
            seoTitle,
            MaximumSeoTitleLength,
            nameof(seoTitle),
            "Moment tag SEO title");
        TaxonomyDomainRules.ValidateOptionalText(
            seoDescription,
            MaximumSeoDescriptionLength,
            nameof(seoDescription),
            "Moment tag SEO description");
    }
}

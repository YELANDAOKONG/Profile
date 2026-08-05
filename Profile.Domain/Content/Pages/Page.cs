using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Pages.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Pages;

public sealed class Page
{
    public const int MaximumTitleLength = 256;
    public const int MaximumSeoTitleLength = 128;
    public const int MaximumSeoDescriptionLength = 512;
    public const int MaximumCountPerAccount = 1_024;

    private Page(
        PageIdentity id,
        UserIdentity authorId,
        PageRouteIdentifier routeIdentifier,
        string title,
        ContentBlockCollection blocks,
        ContentVisibility visibility,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        string? seoTitle,
        string? seoDescription,
        MediaReference? featuredMedia,
        PagePublication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        CommentModerationPolicy? commentModerationPolicyOverride)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(routeIdentifier);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(publication);

        ValidateTitle(title);
        ValidateVisibility(visibility);
        ValidateCommenterPolicy(commenterPolicy);
        ValidateCommentModerationPolicyOverride(
            commentModerationPolicyOverride);
        ValidateOptionalMaximumLength(
            seoTitle,
            MaximumSeoTitleLength,
            nameof(seoTitle),
            "SEO title");
        ValidateOptionalMaximumLength(
            seoDescription,
            MaximumSeoDescriptionLength,
            nameof(seoDescription),
            "SEO description");

        if (updatedAt < createdAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAt),
                updatedAt,
                "Updated time cannot be earlier than created time.");
        }

        ValidateStateTimestamp(
            publication.FirstPublishedAt,
            createdAt,
            updatedAt,
            nameof(publication));
        ValidateStateTimestamp(
            publication.LastPublishedAt,
            createdAt,
            updatedAt,
            nameof(publication));
        ValidateStateTimestamp(
            deletion?.DeletedAt,
            createdAt,
            updatedAt,
            nameof(deletion));

        Id = id;
        AuthorId = authorId;
        RouteIdentifier = routeIdentifier;
        Title = title;
        Blocks = blocks;
        Visibility = visibility;
        CommentsAllowed = commentsAllowed;
        CommenterPolicy = commenterPolicy;
        CommentModerationPolicyOverride = commentModerationPolicyOverride;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        FeaturedMedia = featuredMedia;
        Publication = publication;
        Deletion = deletion;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public PageIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public PageRouteIdentifier RouteIdentifier { get; private set; }

    public string Title { get; private set; }

    public ContentBlockCollection Blocks { get; private set; }

    public ContentVisibility Visibility { get; private set; }

    public bool CommentsAllowed { get; private set; }

    public CommenterPolicy CommenterPolicy { get; private set; }

    public CommentModerationPolicy? CommentModerationPolicyOverride
    {
        get;
        private set;
    }

    public string? SeoTitle { get; private set; }

    public string? SeoDescription { get; private set; }

    public MediaReference? FeaturedMedia { get; private set; }

    public PagePublication Publication { get; private set; }

    public ContentDeletion? Deletion { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Page Create(
        PageIdentity id,
        UserIdentity authorId,
        PageRouteIdentifier routeIdentifier,
        string title,
        ContentBlockCollection blocks,
        ContentVisibility visibility,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        string? seoTitle,
        string? seoDescription,
        MediaReference? featuredMedia,
        DateTimeOffset createdAt,
        CommentModerationPolicy? commentModerationPolicyOverride = null) =>
        new(
            id,
            authorId,
            routeIdentifier,
            title,
            blocks,
            visibility,
            commentsAllowed,
            commenterPolicy,
            seoTitle,
            seoDescription,
            featuredMedia,
            PagePublication.CreateDraft(),
            null,
            createdAt,
            createdAt,
            commentModerationPolicyOverride);

    public static Page Reconstitute(
        PageIdentity id,
        UserIdentity authorId,
        PageRouteIdentifier routeIdentifier,
        string title,
        ContentBlockCollection blocks,
        ContentVisibility visibility,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        string? seoTitle,
        string? seoDescription,
        MediaReference? featuredMedia,
        PagePublication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        CommentModerationPolicy? commentModerationPolicyOverride = null) =>
        new(
            id,
            authorId,
            routeIdentifier,
            title,
            blocks,
            visibility,
            commentsAllowed,
            commenterPolicy,
            seoTitle,
            seoDescription,
            featuredMedia,
            publication,
            deletion,
            createdAt,
            updatedAt,
            commentModerationPolicyOverride);

    public PageRouteReservation? ChangeRouteIdentifier(
        PageRouteIdentifier routeIdentifier,
        DateTimeOffset changedAt) =>
        ChangeRouteIdentifier(
            routeIdentifier,
            changedAt,
            PageRouteReservation.DefaultReservationPeriod);

    public PageRouteReservation? ChangeRouteIdentifier(
        PageRouteIdentifier routeIdentifier,
        DateTimeOffset changedAt,
        TimeSpan reservationPeriod)
    {
        ArgumentNullException.ThrowIfNull(routeIdentifier);

        EnsureCanEdit(changedAt, nameof(changedAt));

        if (reservationPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reservationPeriod),
                reservationPeriod,
                "Page route reservation period must be greater than zero.");
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

        var reservation = PageRouteReservation.Create(
            Id,
            AuthorId,
            RouteIdentifier,
            changedAt,
            reservationPeriod);

        RouteIdentifier = routeIdentifier;
        UpdatedAt = changedAt;

        return reservation;
    }

    public void UpdateContent(
        string title,
        ContentBlockCollection blocks,
        MediaReference? featuredMedia,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(blocks);

        EnsureCanEdit(changedAt, nameof(changedAt));
        ValidateTitle(title);

        Title = title;
        Blocks = blocks;
        FeaturedMedia = featuredMedia;
        UpdatedAt = changedAt;
    }

    public void ChangeVisibility(
        ContentVisibility visibility,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(changedAt, nameof(changedAt));
        ValidateVisibility(visibility);

        Visibility = visibility;
        UpdatedAt = changedAt;
    }

    public void ChangeDiscussion(
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(changedAt, nameof(changedAt));
        ValidateCommenterPolicy(commenterPolicy);

        CommentsAllowed = commentsAllowed;
        CommenterPolicy = commenterPolicy;
        UpdatedAt = changedAt;
    }

    public void ChangeCommentModerationPolicyOverride(
        CommentModerationPolicy? policy,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(changedAt, nameof(changedAt));
        ValidateCommentModerationPolicyOverride(policy);

        CommentModerationPolicyOverride = policy;
        UpdatedAt = changedAt;
    }

    public void UpdateSearchMetadata(
        string? seoTitle,
        string? seoDescription,
        DateTimeOffset changedAt)
    {
        EnsureCanEdit(changedAt, nameof(changedAt));
        ValidateOptionalMaximumLength(
            seoTitle,
            MaximumSeoTitleLength,
            nameof(seoTitle),
            "SEO title");
        ValidateOptionalMaximumLength(
            seoDescription,
            MaximumSeoDescriptionLength,
            nameof(seoDescription),
            "SEO description");

        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        UpdatedAt = changedAt;
    }

    public void Publish(DateTimeOffset publishedAt)
    {
        EnsureCanChangePublication(publishedAt, nameof(publishedAt));
        ValidateTitle(Title);

        Publication = Publication.Publish(publishedAt);
        UpdatedAt = publishedAt;
    }

    public void UnpublishToDraft(DateTimeOffset changedAt)
    {
        EnsureCanChangePublication(changedAt, nameof(changedAt));

        Publication = Publication.Unpublish();
        UpdatedAt = changedAt;
    }

    public void Delete(DateTimeOffset deletedAt)
    {
        EnsureMutationTime(deletedAt, nameof(deletedAt));

        if (Deletion is not null)
        {
            throw new InvalidOperationException(
                "The page is already deleted.");
        }

        Deletion = ContentDeletion.Create(deletedAt);
        UpdatedAt = deletedAt;
    }

    public void Restore(DateTimeOffset restoredAt)
    {
        EnsureMutationTime(restoredAt, nameof(restoredAt));

        if (Deletion is not { } deletion)
        {
            throw new InvalidOperationException(
                "The page is not deleted.");
        }

        if (!deletion.CanRestoreAt(restoredAt))
        {
            throw new InvalidOperationException(
                "The page cannot be restored after its recovery period has ended.");
        }

        Deletion = null;
        UpdatedAt = restoredAt;
    }

    public void UnpublishAndDiscard(DateTimeOffset deletedAt)
    {
        EnsureCanChangePublication(deletedAt, nameof(deletedAt));

        var publication = Publication.Unpublish();
        var deletion = ContentDeletion.Create(deletedAt);

        Publication = publication;
        Deletion = deletion;
        UpdatedAt = deletedAt;
    }

    public bool CanRestoreAt(DateTimeOffset timestamp) =>
        Deletion?.CanRestoreAt(timestamp) ?? false;

    public bool IsReadyForPurgeAt(DateTimeOffset timestamp) =>
        Deletion?.IsReadyForPurgeAt(timestamp) ?? false;

    private void EnsureCanEdit(DateTimeOffset changedAt, string parameterName)
    {
        if (Deletion is not null)
        {
            throw new InvalidOperationException(
                "A deleted page cannot be edited.");
        }

        EnsureMutationTime(changedAt, parameterName);
    }

    private void EnsureCanChangePublication(
        DateTimeOffset changedAt,
        string parameterName)
    {
        if (Deletion is not null)
        {
            throw new InvalidOperationException(
                "A deleted page cannot change publication state.");
        }

        EnsureMutationTime(changedAt, parameterName);
    }

    private void EnsureMutationTime(
        DateTimeOffset changedAt,
        string parameterName)
    {
        if (changedAt < UpdatedAt)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                changedAt,
                "Change time cannot be earlier than the page's updated time.");
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Page title cannot be empty or whitespace.",
                nameof(title));
        }

        if (title.Length > MaximumTitleLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(title),
                title.Length,
                $"Page title length cannot exceed {MaximumTitleLength} characters.");
        }
    }

    private static void ValidateVisibility(ContentVisibility visibility)
    {
        if (!Enum.IsDefined(visibility))
        {
            throw new ArgumentOutOfRangeException(
                nameof(visibility),
                visibility,
                "Content visibility is not supported.");
        }
    }

    private static void ValidateCommenterPolicy(
        CommenterPolicy commenterPolicy)
    {
        if (!Enum.IsDefined(commenterPolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(commenterPolicy),
                commenterPolicy,
                "Commenter policy is not supported.");
        }
    }

    private static void ValidateCommentModerationPolicyOverride(
        CommentModerationPolicy? policy)
    {
        if (policy is not null && !Enum.IsDefined(policy.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(policy),
                policy,
                "Comment moderation policy is not supported.");
        }
    }

    private static void ValidateOptionalMaximumLength(
        string? value,
        int maximumLength,
        string parameterName,
        string displayName)
    {
        if (value is not null && value.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value.Length,
                $"{displayName} length cannot exceed {maximumLength} characters.");
        }
    }

    private static void ValidateStateTimestamp(
        DateTimeOffset? stateTimestamp,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        string parameterName)
    {
        if (stateTimestamp < createdAt || stateTimestamp > updatedAt)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                stateTimestamp,
                "Page state time must be between created and updated times.");
        }
    }
}

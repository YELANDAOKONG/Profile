using System.Collections.ObjectModel;

using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Categories.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Blogs;

public sealed class Blog
{
    public const int MaximumTitleLength = 256;
    public const int MaximumSummaryLength = 2_048;
    public const int MaximumTagCount = 32;
    public const int MaximumSeoTitleLength = 128;
    public const int MaximumSeoDescriptionLength = 512;
    public const int MaximumCoAuthorCount = 32;

    private ReadOnlyCollection<BlogTagIdentity> _tagIds;
    private ReadOnlyCollection<CoAuthor> _coAuthors;

    private Blog(
        BlogIdentity id,
        UserIdentity authorId,
        BlogSlug slug,
        string title,
        ContentBlockCollection blocks,
        string? summary,
        MediaReference? featuredMedia,
        ContentVisibility visibility,
        CategoryIdentity? categoryId,
        IEnumerable<BlogTagIdentity> tagIds,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        bool pinned,
        bool featured,
        string? seoTitle,
        string? seoDescription,
        string? canonicalUrl,
        IEnumerable<CoAuthor> coAuthors,
        Publication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(tagIds);
        ArgumentNullException.ThrowIfNull(coAuthors);
        ArgumentNullException.ThrowIfNull(publication);

        ValidateTitle(title);
        ValidateOptionalMaximumLength(
            summary,
            MaximumSummaryLength,
            nameof(summary),
            "Blog summary");
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
        ValidateVisibility(visibility);
        ValidateCommenterPolicy(commenterPolicy);

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

        _tagIds = CopyTagIds(tagIds);
        _coAuthors = CopyCoAuthors(coAuthors);

        Id = id;
        AuthorId = authorId;
        Slug = slug;
        Title = title;
        Blocks = blocks;
        Summary = summary;
        FeaturedMedia = featuredMedia;
        Visibility = visibility;
        CategoryId = categoryId;
        CommentsAllowed = commentsAllowed;
        CommenterPolicy = commenterPolicy;
        Pinned = pinned;
        Featured = featured;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        CanonicalUrl = canonicalUrl;
        Publication = publication;
        Deletion = deletion;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public BlogIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public BlogSlug Slug { get; }

    public string Title { get; private set; }

    public ContentBlockCollection Blocks { get; private set; }

    public string? Summary { get; private set; }

    public MediaReference? FeaturedMedia { get; private set; }

    public ContentVisibility Visibility { get; private set; }

    public CategoryIdentity? CategoryId { get; private set; }

    public IReadOnlyList<BlogTagIdentity> TagIds => _tagIds;

    public bool CommentsAllowed { get; private set; }

    public CommenterPolicy CommenterPolicy { get; private set; }

    public bool Pinned { get; private set; }

    public bool Featured { get; private set; }

    public string? SeoTitle { get; private set; }

    public string? SeoDescription { get; private set; }

    public string? CanonicalUrl { get; private set; }

    public IReadOnlyList<CoAuthor> CoAuthors => _coAuthors;

    public Publication Publication { get; private set; }

    public ContentDeletion? Deletion { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Blog Create(
        BlogIdentity id,
        UserIdentity authorId,
        BlogSlug slug,
        string title,
        ContentBlockCollection blocks,
        string? summary,
        MediaReference? featuredMedia,
        ContentVisibility visibility,
        CategoryIdentity? categoryId,
        IEnumerable<BlogTagIdentity> tagIds,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        bool pinned,
        bool featured,
        string? seoTitle,
        string? seoDescription,
        string? canonicalUrl,
        IEnumerable<CoAuthor> coAuthors,
        DateTimeOffset createdAt) =>
        new(
            id,
            authorId,
            slug,
            title,
            blocks,
            summary,
            featuredMedia,
            visibility,
            categoryId,
            tagIds,
            commentsAllowed,
            commenterPolicy,
            pinned,
            featured,
            seoTitle,
            seoDescription,
            canonicalUrl,
            coAuthors,
            Publication.CreateDraft(),
            null,
            createdAt,
            createdAt);

    public static Blog Reconstitute(
        BlogIdentity id,
        UserIdentity authorId,
        BlogSlug slug,
        string title,
        ContentBlockCollection blocks,
        string? summary,
        MediaReference? featuredMedia,
        ContentVisibility visibility,
        CategoryIdentity? categoryId,
        IEnumerable<BlogTagIdentity> tagIds,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        bool pinned,
        bool featured,
        string? seoTitle,
        string? seoDescription,
        string? canonicalUrl,
        IEnumerable<CoAuthor> coAuthors,
        Publication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(
            id,
            authorId,
            slug,
            title,
            blocks,
            summary,
            featuredMedia,
            visibility,
            categoryId,
            tagIds,
            commentsAllowed,
            commenterPolicy,
            pinned,
            featured,
            seoTitle,
            seoDescription,
            canonicalUrl,
            coAuthors,
            publication,
            deletion,
            createdAt,
            updatedAt);

    public void Schedule(DateTimeOffset scheduledAt, DateTimeOffset changedAt)
    {
        EnsureCanChangePublication(changedAt, nameof(changedAt));

        Publication = Publication.Schedule(scheduledAt, changedAt);
        UpdatedAt = changedAt;
    }

    public void Unschedule(DateTimeOffset changedAt)
    {
        EnsureCanChangePublication(changedAt, nameof(changedAt));

        Publication = Publication.Unschedule();
        UpdatedAt = changedAt;
    }

    public void SubmitForReview(DateTimeOffset changedAt)
    {
        EnsureCanChangePublication(changedAt, nameof(changedAt));
        ValidateTitle(Title);

        Publication = Publication.SubmitForReview();
        UpdatedAt = changedAt;
    }

    public void Approve(DateTimeOffset publishedAt)
    {
        EnsureCanChangePublication(publishedAt, nameof(publishedAt));
        ValidateTitle(Title);

        Publication = Publication.Approve(publishedAt);
        UpdatedAt = publishedAt;
    }

    public void PublishScheduled(DateTimeOffset publishedAt)
    {
        EnsureCanChangePublication(publishedAt, nameof(publishedAt));
        ValidateTitle(Title);

        Publication = Publication.PublishScheduled(publishedAt);
        UpdatedAt = publishedAt;
    }

    public void ReturnToDraft(DateTimeOffset changedAt)
    {
        EnsureCanChangePublication(changedAt, nameof(changedAt));

        Publication = Publication.ReturnToDraft();
        UpdatedAt = changedAt;
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
                "The blog is already deleted.");
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
                "The blog is not deleted.");
        }

        if (!deletion.CanRestoreAt(restoredAt))
        {
            throw new InvalidOperationException(
                "The blog cannot be restored after its recovery period has ended.");
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

    private void EnsureCanChangePublication(
        DateTimeOffset changedAt,
        string parameterName)
    {
        if (Deletion is not null)
        {
            throw new InvalidOperationException(
                "A deleted blog cannot change publication state.");
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
                "Change time cannot be earlier than the blog's updated time.");
        }
    }

    private static ReadOnlyCollection<BlogTagIdentity> CopyTagIds(
        IEnumerable<BlogTagIdentity> tagIds)
    {
        var items = tagIds.ToArray();

        if (items.Length > MaximumTagCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tagIds),
                items.Length,
                $"Blog tag count cannot exceed {MaximumTagCount}.");
        }

        if (items.Any(static tagId => tagId is null))
        {
            throw new ArgumentException(
                "Blog tags cannot contain a null item.",
                nameof(tagIds));
        }

        if (items.Distinct().Count() != items.Length)
        {
            throw new ArgumentException(
                "Blog tags cannot contain duplicate identities.",
                nameof(tagIds));
        }

        return Array.AsReadOnly(items);
    }

    private static ReadOnlyCollection<CoAuthor> CopyCoAuthors(
        IEnumerable<CoAuthor> coAuthors)
    {
        var items = coAuthors.ToArray();

        if (items.Length > MaximumCoAuthorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(coAuthors),
                items.Length,
                $"Blog co-author count cannot exceed {MaximumCoAuthorCount}.");
        }

        if (items.Any(static coAuthor => coAuthor is null))
        {
            throw new ArgumentException(
                "Blog co-authors cannot contain a null item.",
                nameof(coAuthors));
        }

        if (items.Distinct().Count() != items.Length)
        {
            throw new ArgumentException(
                "Blog co-authors cannot contain duplicate values.",
                nameof(coAuthors));
        }

        return Array.AsReadOnly(items);
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Blog title cannot be empty or whitespace.",
                nameof(title));
        }

        if (title.Length > MaximumTitleLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(title),
                title.Length,
                $"Blog title length cannot exceed {MaximumTitleLength} characters.");
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

    private static void ValidateCommenterPolicy(CommenterPolicy commenterPolicy)
    {
        if (!Enum.IsDefined(commenterPolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(commenterPolicy),
                commenterPolicy,
                "Commenter policy is not supported.");
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
                "Blog state time must be between created and updated times.");
        }
    }
}

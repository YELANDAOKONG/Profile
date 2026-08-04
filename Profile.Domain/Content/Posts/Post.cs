using System.Collections.ObjectModel;

using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Posts;

public sealed class Post
{
    public const int MaximumBodyLength = 8_192;
    public const int MaximumMediaCount = 9;
    public const int MaximumAudienceAccountCount = 2_048;
    public const int MaximumTagCount = 32;

    private ReadOnlyCollection<MediaReference> _media;
    private ReadOnlyCollection<UserIdentity> _audienceAccountIds;
    private ReadOnlyCollection<PostTagIdentity> _tagIds;

    private Post(
        PostIdentity id,
        UserIdentity authorId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        ContentVisibility visibility,
        AudienceRestrictionMode audienceRestrictionMode,
        IEnumerable<UserIdentity> audienceAccountIds,
        PostIdentity? quotedPostId,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        IEnumerable<PostTagIdentity> tagIds,
        Publication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(authorId);
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(audienceAccountIds);
        ArgumentNullException.ThrowIfNull(tagIds);
        ArgumentNullException.ThrowIfNull(publication);

        if (quotedPostId == id)
        {
            throw new ArgumentException(
                "A Post cannot quote itself by identity.",
                nameof(quotedPostId));
        }

        ValidateBody(body);
        ValidateVisibility(visibility);
        ValidateAudienceRestrictionMode(audienceRestrictionMode);
        ValidateCommenterPolicy(commenterPolicy);

        var copiedMedia = CopyMedia(media);
        var copiedAudienceAccountIds = CopyAudienceAccountIds(
            authorId,
            audienceAccountIds);
        var copiedTagIds = CopyTagIds(tagIds);

        ValidateContent(body, copiedMedia);

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
        Body = body;
        _media = copiedMedia;
        Visibility = visibility;
        AudienceRestrictionMode = audienceRestrictionMode;
        _audienceAccountIds = copiedAudienceAccountIds;
        QuotedPostId = quotedPostId;
        CommentsAllowed = commentsAllowed;
        CommenterPolicy = commenterPolicy;
        _tagIds = copiedTagIds;
        Publication = publication;
        Deletion = deletion;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public PostIdentity Id { get; }

    public UserIdentity AuthorId { get; }

    public ContentBody? Body { get; private set; }

    public IReadOnlyList<MediaReference> Media => _media;

    public ContentVisibility Visibility { get; private set; }

    public AudienceRestrictionMode AudienceRestrictionMode { get; private set; }

    public IReadOnlyList<UserIdentity> AudienceAccountIds =>
        _audienceAccountIds;

    public PostIdentity? QuotedPostId { get; }

    public bool CommentsAllowed { get; private set; }

    public CommenterPolicy CommenterPolicy { get; private set; }

    public IReadOnlyList<PostTagIdentity> TagIds => _tagIds;

    public Publication Publication { get; private set; }

    public ContentDeletion? Deletion { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsPubliclyShareable =>
        Deletion is null &&
        Publication.Status is PublicationStatus.Published &&
        Visibility is ContentVisibility.Public &&
        AudienceRestrictionMode is AudienceRestrictionMode.Blacklist &&
        _audienceAccountIds.Count == 0;

    public static Post Create(
        PostIdentity id,
        UserIdentity authorId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        ContentVisibility visibility,
        AudienceRestrictionMode audienceRestrictionMode,
        IEnumerable<UserIdentity> audienceAccountIds,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        IEnumerable<PostTagIdentity> tagIds,
        DateTimeOffset createdAt) =>
        new(
            id,
            authorId,
            body,
            media,
            visibility,
            audienceRestrictionMode,
            audienceAccountIds,
            null,
            commentsAllowed,
            commenterPolicy,
            tagIds,
            Publication.CreateDraft(),
            null,
            createdAt,
            createdAt);

    public static Post CreateQuote(
        PostIdentity id,
        UserIdentity authorId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        ContentVisibility visibility,
        AudienceRestrictionMode audienceRestrictionMode,
        IEnumerable<UserIdentity> audienceAccountIds,
        Post quotedPost,
        bool isBlockedBetweenAuthors,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        IEnumerable<PostTagIdentity> tagIds,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(quotedPost);

        quotedPost.EnsureCanBeShared(
            isBlockedBetweenAuthors,
            nameof(quotedPost));

        return new Post(
            id,
            authorId,
            body,
            media,
            visibility,
            audienceRestrictionMode,
            audienceAccountIds,
            quotedPost.Id,
            commentsAllowed,
            commenterPolicy,
            tagIds,
            Publication.CreateDraft(),
            null,
            createdAt,
            createdAt);
    }

    public static Post Reconstitute(
        PostIdentity id,
        UserIdentity authorId,
        ContentBody? body,
        IEnumerable<MediaReference> media,
        ContentVisibility visibility,
        AudienceRestrictionMode audienceRestrictionMode,
        IEnumerable<UserIdentity> audienceAccountIds,
        PostIdentity? quotedPostId,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        IEnumerable<PostTagIdentity> tagIds,
        Publication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(
            id,
            authorId,
            body,
            media,
            visibility,
            audienceRestrictionMode,
            audienceAccountIds,
            quotedPostId,
            commentsAllowed,
            commenterPolicy,
            tagIds,
            publication,
            deletion,
            createdAt,
            updatedAt);

    public void UpdateContent(
        ContentBody? body,
        IEnumerable<MediaReference> media,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(media);

        EnsureCanEditContent(changedAt, nameof(changedAt));
        ValidateBody(body);

        var copiedMedia = CopyMedia(media);

        ValidateContent(body, copiedMedia);

        Body = body;
        _media = copiedMedia;
        UpdatedAt = changedAt;
    }

    public void ChangeVisibility(
        ContentVisibility visibility,
        DateTimeOffset changedAt)
    {
        EnsureCanChangeSettings(changedAt, nameof(changedAt));
        ValidateVisibility(visibility);

        Visibility = visibility;
        UpdatedAt = changedAt;
    }

    public void ChangeAudienceRestriction(
        AudienceRestrictionMode mode,
        IEnumerable<UserIdentity> audienceAccountIds,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(audienceAccountIds);

        EnsureCanChangeSettings(changedAt, nameof(changedAt));
        ValidateAudienceRestrictionMode(mode);

        var copiedAudienceAccountIds = CopyAudienceAccountIds(
            AuthorId,
            audienceAccountIds);

        AudienceRestrictionMode = mode;
        _audienceAccountIds = copiedAudienceAccountIds;
        UpdatedAt = changedAt;
    }

    public void ChangeDiscussion(
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        DateTimeOffset changedAt)
    {
        EnsureCanChangeSettings(changedAt, nameof(changedAt));
        ValidateCommenterPolicy(commenterPolicy);

        CommentsAllowed = commentsAllowed;
        CommenterPolicy = commenterPolicy;
        UpdatedAt = changedAt;
    }

    public void ChangeTags(
        IEnumerable<PostTagIdentity> tagIds,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(tagIds);

        EnsureCanChangeSettings(changedAt, nameof(changedAt));

        var copiedTagIds = CopyTagIds(tagIds);

        _tagIds = copiedTagIds;
        UpdatedAt = changedAt;
    }

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
        ValidateContent(Body, _media);

        Publication = Publication.SubmitForReview();
        UpdatedAt = changedAt;
    }

    public void Approve(DateTimeOffset publishedAt)
    {
        EnsureCanChangePublication(publishedAt, nameof(publishedAt));
        ValidateContent(Body, _media);

        Publication = Publication.Approve(publishedAt);
        UpdatedAt = publishedAt;
    }

    public void PublishScheduled(DateTimeOffset publishedAt)
    {
        EnsureCanChangePublication(publishedAt, nameof(publishedAt));
        ValidateContent(Body, _media);

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
                "The Post is already deleted.");
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
                "The Post is not deleted.");
        }

        if (!deletion.CanRestoreAt(restoredAt))
        {
            throw new InvalidOperationException(
                "The Post cannot be restored after its recovery period has ended.");
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

    internal void EnsureCanBeShared(
        bool isBlockedBetweenAuthors,
        string parameterName)
    {
        if (isBlockedBetweenAuthors)
        {
            throw new InvalidOperationException(
                "A Post cannot be reposted or quoted while either author blocks the other.");
        }

        if (!IsPubliclyShareable)
        {
            throw new ArgumentException(
                "Only an active, published Post with an unrestricted Public audience can be reposted or quoted.",
                parameterName);
        }
    }

    private void EnsureCanEditContent(
        DateTimeOffset changedAt,
        string parameterName)
    {
        EnsureCanChangeSettings(changedAt, parameterName);

        if (Publication.Status is PublicationStatus.Published)
        {
            throw new InvalidOperationException(
                "Published Post content cannot be edited before it is unpublished.");
        }
    }

    private void EnsureCanChangeSettings(
        DateTimeOffset changedAt,
        string parameterName)
    {
        if (Deletion is not null)
        {
            throw new InvalidOperationException(
                "A deleted Post cannot be changed.");
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
                "A deleted Post cannot change publication state.");
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
                "Change time cannot be earlier than the Post's updated time.");
        }
    }

    private static ReadOnlyCollection<MediaReference> CopyMedia(
        IEnumerable<MediaReference> media)
    {
        var items = media.ToArray();

        if (items.Length > MaximumMediaCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(media),
                items.Length,
                $"Post media count cannot exceed {MaximumMediaCount}.");
        }

        if (items.Any(static item => item is null))
        {
            throw new ArgumentException(
                "Post media cannot contain a null item.",
                nameof(media));
        }

        if (items.Select(static item => item.MediaId).Distinct().Count() !=
            items.Length)
        {
            throw new ArgumentException(
                "Post media cannot contain duplicate media identities.",
                nameof(media));
        }

        return Array.AsReadOnly(items);
    }

    private static ReadOnlyCollection<UserIdentity> CopyAudienceAccountIds(
        UserIdentity authorId,
        IEnumerable<UserIdentity> audienceAccountIds)
    {
        var items = audienceAccountIds.ToArray();

        if (items.Length > MaximumAudienceAccountCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(audienceAccountIds),
                items.Length,
                $"Post audience account count cannot exceed {MaximumAudienceAccountCount}.");
        }

        if (items.Any(static accountId => accountId is null))
        {
            throw new ArgumentException(
                "Post audience accounts cannot contain a null item.",
                nameof(audienceAccountIds));
        }

        if (items.Distinct().Count() != items.Length)
        {
            throw new ArgumentException(
                "Post audience accounts cannot contain duplicate identities.",
                nameof(audienceAccountIds));
        }

        if (items.Contains(authorId))
        {
            throw new ArgumentException(
                "Post audience accounts cannot contain the author.",
                nameof(audienceAccountIds));
        }

        return Array.AsReadOnly(items);
    }

    private static ReadOnlyCollection<PostTagIdentity> CopyTagIds(
        IEnumerable<PostTagIdentity> tagIds)
    {
        var items = tagIds.ToArray();

        if (items.Length > MaximumTagCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tagIds),
                items.Length,
                $"Post tag count cannot exceed {MaximumTagCount}.");
        }

        if (items.Any(static tagId => tagId is null))
        {
            throw new ArgumentException(
                "Post tags cannot contain a null item.",
                nameof(tagIds));
        }

        if (items.Distinct().Count() != items.Length)
        {
            throw new ArgumentException(
                "Post tags cannot contain duplicate identities.",
                nameof(tagIds));
        }

        return Array.AsReadOnly(items);
    }

    private static void ValidateBody(ContentBody? body)
    {
        if (body is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(body.Source))
        {
            throw new ArgumentException(
                "Post body cannot be empty or whitespace when provided.",
                nameof(body));
        }

        if (body.Source.Length > MaximumBodyLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(body),
                body.Source.Length,
                $"Post body length cannot exceed {MaximumBodyLength} characters.");
        }
    }

    private static void ValidateContent(
        ContentBody? body,
        IReadOnlyCollection<MediaReference> media)
    {
        if (body is null && media.Count == 0)
        {
            throw new ArgumentException(
                "A Post must contain a body or at least one media item.",
                nameof(body));
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

    private static void ValidateAudienceRestrictionMode(
        AudienceRestrictionMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Audience restriction mode is not supported.");
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
                "Post state time must be between created and updated times.");
        }
    }
}

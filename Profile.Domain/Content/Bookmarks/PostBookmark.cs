using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Collections;
using Profile.Domain.Content.Bookmarks.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Bookmarks;

public sealed class PostBookmark
{
    private PostBookmark(
        UserIdentity ownerId,
        PostIdentity postId,
        PostBookmarkFolderIdentity? folderId,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(postId);

        CollectionDomainRules.ValidateSortOrder(sortOrder);
        CollectionDomainRules.ValidateTimestamps(createdAt, updatedAt);

        OwnerId = ownerId;
        PostId = postId;
        FolderId = folderId;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public UserIdentity OwnerId { get; }

    public PostIdentity PostId { get; }

    public PostBookmarkFolderIdentity? FolderId { get; private set; }

    public long SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static PostBookmark Create(
        UserIdentity ownerId,
        PostIdentity postId,
        PostBookmarkFolder? folder,
        long sortOrder,
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(postId);

        EnsureFolderOwnership(ownerId, folder);
        SavedContentEligibilityPolicy.EnsureCanSave(
            isTargetActiveAndPublished,
            isInReadingAudience,
            isBlockedBetweenAccounts);

        return new PostBookmark(
            ownerId,
            postId,
            folder?.Id,
            sortOrder,
            createdAt,
            createdAt);
    }

    public static PostBookmark Reconstitute(
        UserIdentity ownerId,
        PostIdentity postId,
        PostBookmarkFolderIdentity? folderId,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(ownerId, postId, folderId, sortOrder, createdAt, updatedAt);

    public void MoveToFolder(
        PostBookmarkFolder? folder,
        long sortOrder,
        DateTimeOffset changedAt)
    {
        CollectionDomainRules.EnsureMutationTime(changedAt, UpdatedAt);
        EnsureFolderOwnership(OwnerId, folder);
        CollectionDomainRules.ValidateSortOrder(sortOrder);

        FolderId = folder?.Id;
        SortOrder = sortOrder;
        UpdatedAt = changedAt;
    }

    public void Reorder(long sortOrder, DateTimeOffset changedAt)
    {
        CollectionDomainRules.EnsureMutationTime(changedAt, UpdatedAt);
        CollectionDomainRules.ValidateSortOrder(sortOrder);

        SortOrder = sortOrder;
        UpdatedAt = changedAt;
    }

    private static void EnsureFolderOwnership(
        UserIdentity ownerId,
        PostBookmarkFolder? folder)
    {
        if (folder is not null && folder.OwnerId != ownerId)
        {
            throw new ArgumentException(
                "A Post bookmark folder must belong to the bookmark owner.",
                nameof(folder));
        }
    }
}

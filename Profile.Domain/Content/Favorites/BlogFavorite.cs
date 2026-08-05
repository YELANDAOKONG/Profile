using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Collections;
using Profile.Domain.Content.Favorites.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Favorites;

public sealed class BlogFavorite
{
    private BlogFavorite(
        UserIdentity ownerId,
        BlogIdentity blogId,
        BlogFavoriteFolderIdentity? folderId,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(blogId);

        CollectionDomainRules.ValidateSortOrder(sortOrder);
        CollectionDomainRules.ValidateTimestamps(createdAt, updatedAt);

        OwnerId = ownerId;
        BlogId = blogId;
        FolderId = folderId;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public UserIdentity OwnerId { get; }

    public BlogIdentity BlogId { get; }

    public BlogFavoriteFolderIdentity? FolderId { get; private set; }

    public long SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static BlogFavorite Create(
        UserIdentity ownerId,
        BlogIdentity blogId,
        BlogFavoriteFolder? folder,
        long sortOrder,
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(blogId);

        EnsureFolderOwnership(ownerId, folder);
        SavedContentEligibilityPolicy.EnsureCanSave(
            isTargetActiveAndPublished,
            isInReadingAudience,
            isBlockedBetweenAccounts);

        return new BlogFavorite(
            ownerId,
            blogId,
            folder?.Id,
            sortOrder,
            createdAt,
            createdAt);
    }

    public static BlogFavorite Reconstitute(
        UserIdentity ownerId,
        BlogIdentity blogId,
        BlogFavoriteFolderIdentity? folderId,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(ownerId, blogId, folderId, sortOrder, createdAt, updatedAt);

    public void MoveToFolder(
        BlogFavoriteFolder? folder,
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
        BlogFavoriteFolder? folder)
    {
        if (folder is not null && folder.OwnerId != ownerId)
        {
            throw new ArgumentException(
                "A Blog favorite folder must belong to the favorite owner.",
                nameof(folder));
        }
    }
}

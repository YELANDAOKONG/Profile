using Profile.Domain.Content.Collections;
using Profile.Domain.Content.Collections.Value;
using Profile.Domain.Content.Bookmarks.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Bookmarks;

public sealed class PostBookmarkFolder
{
    private PostBookmarkFolder(
        PostBookmarkFolderIdentity id,
        UserIdentity ownerId,
        CollectionFolderName name,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(name);

        CollectionDomainRules.ValidateSortOrder(sortOrder);
        CollectionDomainRules.ValidateTimestamps(createdAt, updatedAt);

        Id = id;
        OwnerId = ownerId;
        Name = name;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public PostBookmarkFolderIdentity Id { get; }

    public UserIdentity OwnerId { get; }

    public CollectionFolderName Name { get; private set; }

    public long SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static PostBookmarkFolder Create(
        PostBookmarkFolderIdentity id,
        UserIdentity ownerId,
        CollectionFolderName name,
        long sortOrder,
        DateTimeOffset createdAt) =>
        new(id, ownerId, name, sortOrder, createdAt, createdAt);

    public static PostBookmarkFolder Reconstitute(
        PostBookmarkFolderIdentity id,
        UserIdentity ownerId,
        CollectionFolderName name,
        long sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(id, ownerId, name, sortOrder, createdAt, updatedAt);

    public void Rename(
        CollectionFolderName name,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(name);

        CollectionDomainRules.EnsureMutationTime(changedAt, UpdatedAt);

        Name = name;
        UpdatedAt = changedAt;
    }

    public void Reorder(long sortOrder, DateTimeOffset changedAt)
    {
        CollectionDomainRules.EnsureMutationTime(changedAt, UpdatedAt);
        CollectionDomainRules.ValidateSortOrder(sortOrder);

        SortOrder = sortOrder;
        UpdatedAt = changedAt;
    }
}

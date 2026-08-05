using Profile.Domain.Content.Collections.Value;
using Profile.Domain.Content.Bookmarks;
using Profile.Domain.Content.Bookmarks.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Bookmarks;

public sealed class PostBookmarkFolderTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidState_PreservesValues()
    {
        var id = PostBookmarkFolderIdentity.New();
        var ownerId = UserIdentity.New();
        var name = new CollectionFolderName("Reading");

        var folder = PostBookmarkFolder.Create(
            id,
            ownerId,
            name,
            sortOrder: 3,
            _createdAt);

        Assert.Equal(id, folder.Id);
        Assert.Equal(ownerId, folder.OwnerId);
        Assert.Same(name, folder.Name);
        Assert.Equal(3, folder.SortOrder);
        Assert.Equal(_createdAt, folder.CreatedAt);
        Assert.Equal(_createdAt, folder.UpdatedAt);
    }

    [Fact]
    public void Rename_WithValidTime_ReplacesName()
    {
        var folder = CreateFolder();
        var name = new CollectionFolderName("Bookmarks");
        var changedAt = _createdAt.AddMinutes(1);

        folder.Rename(name, changedAt);

        Assert.Same(name, folder.Name);
        Assert.Equal(changedAt, folder.UpdatedAt);
    }

    [Fact]
    public void Reorder_WithValidValue_ReplacesSortOrder()
    {
        var folder = CreateFolder();
        var changedAt = _createdAt.AddMinutes(1);

        folder.Reorder(7, changedAt);

        Assert.Equal(7, folder.SortOrder);
        Assert.Equal(changedAt, folder.UpdatedAt);
    }

    [Fact]
    public void Reorder_WithNegativeValue_ThrowsAndPreservesState()
    {
        var folder = CreateFolder();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => folder.Reorder(-1, _createdAt.AddMinutes(1)));

        Assert.Equal(0, folder.SortOrder);
        Assert.Equal(_createdAt, folder.UpdatedAt);
    }

    [Fact]
    public void Mutation_WithEarlierTime_ThrowsArgumentOutOfRangeException()
    {
        var folder = CreateFolder();
        var earlier = _createdAt.AddTicks(-1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => folder.Rename(new CollectionFolderName("New"), earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => folder.Reorder(1, earlier));
    }

    [Fact]
    public void Reconstitute_WithUpdatedTimeBeforeCreatedTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PostBookmarkFolder.Reconstitute(
                PostBookmarkFolderIdentity.New(),
                UserIdentity.New(),
                new CollectionFolderName("Reading"),
                0,
                _createdAt,
                _createdAt.AddTicks(-1)));
    }

    private static PostBookmarkFolder CreateFolder(
        UserIdentity? ownerId = null) =>
        PostBookmarkFolder.Create(
            PostBookmarkFolderIdentity.New(),
            ownerId ?? UserIdentity.New(),
            new CollectionFolderName("Reading"),
            0,
            _createdAt);
}

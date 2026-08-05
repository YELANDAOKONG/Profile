using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Collections.Value;
using Profile.Domain.Content.Bookmarks;
using Profile.Domain.Content.Bookmarks.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Bookmarks;

public sealed class PostBookmarkTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithFolder_PreservesValues()
    {
        var ownerId = UserIdentity.New();
        var postId = PostIdentity.New();
        var folder = CreateFolder(ownerId);

        var bookmark = CreateBookmark(ownerId, postId, folder, sortOrder: 3);

        Assert.Equal(ownerId, bookmark.OwnerId);
        Assert.Equal(postId, bookmark.PostId);
        Assert.Equal(folder.Id, bookmark.FolderId);
        Assert.Equal(3, bookmark.SortOrder);
        Assert.Equal(_createdAt, bookmark.CreatedAt);
        Assert.Equal(_createdAt, bookmark.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutFolder_UsesUncategorized()
    {
        var bookmark = CreateBookmark();

        Assert.Null(bookmark.FolderId);
    }

    [Fact]
    public void Create_WithFolderOwnedByAnotherAccount_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateBookmark(folder: CreateFolder(UserIdentity.New())));
    }

    [Theory]
    [InlineData(false, true, false, typeof(ArgumentException))]
    [InlineData(true, false, false, typeof(InvalidOperationException))]
    [InlineData(true, true, true, typeof(InvalidOperationException))]
    public void Create_WithIneligibleTarget_ThrowsExpectedException(
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        Type exceptionType)
    {
        Assert.Throws(
            exceptionType,
            () => PostBookmark.Create(
                UserIdentity.New(),
                PostIdentity.New(),
                null,
                0,
                isTargetActiveAndPublished,
                isInReadingAudience,
                isBlockedBetweenAccounts,
                _createdAt));
    }

    [Fact]
    public void Create_WithNegativeSortOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBookmark(sortOrder: -1));
    }

    [Fact]
    public void MoveToFolder_ChangesFolderAndSortOrder()
    {
        var ownerId = UserIdentity.New();
        var bookmark = CreateBookmark(ownerId: ownerId);
        var folder = CreateFolder(ownerId);
        var changedAt = _createdAt.AddMinutes(1);

        bookmark.MoveToFolder(folder, 5, changedAt);

        Assert.Equal(folder.Id, bookmark.FolderId);
        Assert.Equal(5, bookmark.SortOrder);
        Assert.Equal(changedAt, bookmark.UpdatedAt);
    }

    [Fact]
    public void MoveToFolder_WithNullFolder_MovesToUncategorized()
    {
        var ownerId = UserIdentity.New();
        var bookmark = CreateBookmark(
            ownerId: ownerId,
            folder: CreateFolder(ownerId));

        bookmark.MoveToFolder(null, 2, _createdAt.AddMinutes(1));

        Assert.Null(bookmark.FolderId);
        Assert.Equal(2, bookmark.SortOrder);
    }

    [Fact]
    public void MoveToFolder_WithDifferentOwner_ThrowsAndPreservesState()
    {
        var bookmark = CreateBookmark();

        Assert.Throws<ArgumentException>(
            () => bookmark.MoveToFolder(
                CreateFolder(UserIdentity.New()),
                1,
                _createdAt.AddMinutes(1)));

        Assert.Null(bookmark.FolderId);
        Assert.Equal(0, bookmark.SortOrder);
        Assert.Equal(_createdAt, bookmark.UpdatedAt);
    }

    [Fact]
    public void Reorder_WithValidValue_ReplacesSortOrder()
    {
        var bookmark = CreateBookmark();
        var changedAt = _createdAt.AddMinutes(1);

        bookmark.Reorder(8, changedAt);

        Assert.Equal(8, bookmark.SortOrder);
        Assert.Equal(changedAt, bookmark.UpdatedAt);
    }

    [Fact]
    public void Mutation_WithEarlierTime_ThrowsArgumentOutOfRangeException()
    {
        var bookmark = CreateBookmark();
        var earlier = _createdAt.AddTicks(-1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => bookmark.MoveToFolder(null, 0, earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => bookmark.Reorder(0, earlier));
    }

    [Fact]
    public void Reconstitute_WithUpdatedTimeBeforeCreatedTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PostBookmark.Reconstitute(
                UserIdentity.New(),
                PostIdentity.New(),
                null,
                0,
                _createdAt,
                _createdAt.AddTicks(-1)));
    }

    private static PostBookmark CreateBookmark(
        UserIdentity? ownerId = null,
        PostIdentity? postId = null,
        PostBookmarkFolder? folder = null,
        long sortOrder = 0)
    {
        var effectiveOwnerId = ownerId ?? UserIdentity.New();

        return PostBookmark.Create(
            effectiveOwnerId,
            postId ?? PostIdentity.New(),
            folder,
            sortOrder,
            isTargetActiveAndPublished: true,
            isInReadingAudience: true,
            isBlockedBetweenAccounts: false,
            _createdAt);
    }

    private static PostBookmarkFolder CreateFolder(UserIdentity ownerId) =>
        PostBookmarkFolder.Create(
            PostBookmarkFolderIdentity.New(),
            ownerId,
            new CollectionFolderName("Reading"),
            0,
            _createdAt);
}

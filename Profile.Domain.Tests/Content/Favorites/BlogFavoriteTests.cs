using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Collections.Value;
using Profile.Domain.Content.Favorites;
using Profile.Domain.Content.Favorites.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Favorites;

public sealed class BlogFavoriteTests
{
    private static readonly DateTimeOffset _createdAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithFolder_PreservesValues()
    {
        var ownerId = UserIdentity.New();
        var blogId = BlogIdentity.New();
        var folder = CreateFolder(ownerId);

        var favorite = CreateFavorite(ownerId, blogId, folder, sortOrder: 3);

        Assert.Equal(ownerId, favorite.OwnerId);
        Assert.Equal(blogId, favorite.BlogId);
        Assert.Equal(folder.Id, favorite.FolderId);
        Assert.Equal(3, favorite.SortOrder);
        Assert.Equal(_createdAt, favorite.CreatedAt);
        Assert.Equal(_createdAt, favorite.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutFolder_UsesUncategorized()
    {
        var favorite = CreateFavorite();

        Assert.Null(favorite.FolderId);
    }

    [Fact]
    public void Create_WithFolderOwnedByAnotherAccount_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateFavorite(folder: CreateFolder(UserIdentity.New())));
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
            () => BlogFavorite.Create(
                UserIdentity.New(),
                BlogIdentity.New(),
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
            () => CreateFavorite(sortOrder: -1));
    }

    [Fact]
    public void MoveToFolder_ChangesFolderAndSortOrder()
    {
        var ownerId = UserIdentity.New();
        var favorite = CreateFavorite(ownerId: ownerId);
        var folder = CreateFolder(ownerId);
        var changedAt = _createdAt.AddMinutes(1);

        favorite.MoveToFolder(folder, 5, changedAt);

        Assert.Equal(folder.Id, favorite.FolderId);
        Assert.Equal(5, favorite.SortOrder);
        Assert.Equal(changedAt, favorite.UpdatedAt);
    }

    [Fact]
    public void MoveToFolder_WithNullFolder_MovesToUncategorized()
    {
        var ownerId = UserIdentity.New();
        var favorite = CreateFavorite(
            ownerId: ownerId,
            folder: CreateFolder(ownerId));

        favorite.MoveToFolder(null, 2, _createdAt.AddMinutes(1));

        Assert.Null(favorite.FolderId);
        Assert.Equal(2, favorite.SortOrder);
    }

    [Fact]
    public void MoveToFolder_WithDifferentOwner_ThrowsAndPreservesState()
    {
        var favorite = CreateFavorite();

        Assert.Throws<ArgumentException>(
            () => favorite.MoveToFolder(
                CreateFolder(UserIdentity.New()),
                1,
                _createdAt.AddMinutes(1)));

        Assert.Null(favorite.FolderId);
        Assert.Equal(0, favorite.SortOrder);
        Assert.Equal(_createdAt, favorite.UpdatedAt);
    }

    [Fact]
    public void Reorder_WithValidValue_ReplacesSortOrder()
    {
        var favorite = CreateFavorite();
        var changedAt = _createdAt.AddMinutes(1);

        favorite.Reorder(8, changedAt);

        Assert.Equal(8, favorite.SortOrder);
        Assert.Equal(changedAt, favorite.UpdatedAt);
    }

    [Fact]
    public void Mutation_WithEarlierTime_ThrowsArgumentOutOfRangeException()
    {
        var favorite = CreateFavorite();
        var earlier = _createdAt.AddTicks(-1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => favorite.MoveToFolder(null, 0, earlier));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => favorite.Reorder(0, earlier));
    }

    [Fact]
    public void Reconstitute_WithUpdatedTimeBeforeCreatedTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BlogFavorite.Reconstitute(
                UserIdentity.New(),
                BlogIdentity.New(),
                null,
                0,
                _createdAt,
                _createdAt.AddTicks(-1)));
    }

    private static BlogFavorite CreateFavorite(
        UserIdentity? ownerId = null,
        BlogIdentity? blogId = null,
        BlogFavoriteFolder? folder = null,
        long sortOrder = 0)
    {
        var effectiveOwnerId = ownerId ?? UserIdentity.New();

        return BlogFavorite.Create(
            effectiveOwnerId,
            blogId ?? BlogIdentity.New(),
            folder,
            sortOrder,
            isTargetActiveAndPublished: true,
            isInReadingAudience: true,
            isBlockedBetweenAccounts: false,
            _createdAt);
    }

    private static BlogFavoriteFolder CreateFolder(UserIdentity ownerId) =>
        BlogFavoriteFolder.Create(
            BlogFavoriteFolderIdentity.New(),
            ownerId,
            new CollectionFolderName("Reading"),
            0,
            _createdAt);
}

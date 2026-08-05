using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Categories.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Blogs;

public sealed class BlogEditingTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void UpdateDetails_WithValidValues_ReplacesDetails()
    {
        var blog = CreateBlog();
        var featuredMedia = new MediaReference(
            MediaItemIdentity.New(),
            "Featured image");
        var changedAt = _baseTime.AddMinutes(1);

        blog.UpdateDetails(
            "Changed title",
            "Changed summary",
            featuredMedia,
            changedAt);

        Assert.Equal("Changed title", blog.Title);
        Assert.Equal("Changed summary", blog.Summary);
        Assert.Equal(featuredMedia, blog.FeaturedMedia);
        Assert.Equal(changedAt, blog.UpdatedAt);
    }

    [Fact]
    public void ConfigurationChanges_ReplaceIndependentSettings()
    {
        var blog = CreateBlog();
        var categoryId = CategoryIdentity.New();
        BlogTagIdentity[] tagIds =
        [
            BlogTagIdentity.New(),
            BlogTagIdentity.New()
        ];
        CoAuthor[] coAuthors =
        [
            CoAuthor.FromText("Co-author")
        ];

        blog.ChangeVisibility(
            ContentVisibility.Followers,
            _baseTime.AddMinutes(1));
        blog.ChangeTaxonomy(
            categoryId,
            tagIds,
            _baseTime.AddMinutes(2));
        blog.ChangeDiscussion(
            commentsAllowed: false,
            CommenterPolicy.AuthorOnly,
            _baseTime.AddMinutes(3));
        blog.ChangePlacement(
            pinned: true,
            featured: true,
            _baseTime.AddMinutes(4));
        blog.UpdateSearchMetadata(
            "SEO title",
            "SEO description",
            "https://example.com/canonical",
            _baseTime.AddMinutes(5));
        blog.ChangeCoAuthors(
            coAuthors,
            _baseTime.AddMinutes(6));

        Assert.Equal(ContentVisibility.Followers, blog.Visibility);
        Assert.Equal(categoryId, blog.CategoryId);
        Assert.Equal(tagIds, blog.TagIds);
        Assert.False(blog.CommentsAllowed);
        Assert.Equal(CommenterPolicy.AuthorOnly, blog.CommenterPolicy);
        Assert.True(blog.Pinned);
        Assert.True(blog.Featured);
        Assert.Equal("SEO title", blog.SeoTitle);
        Assert.Equal("SEO description", blog.SeoDescription);
        Assert.Equal("https://example.com/canonical", blog.CanonicalUrl);
        Assert.Equal(coAuthors, blog.CoAuthors);
        Assert.Equal(_baseTime.AddMinutes(6), blog.UpdatedAt);
    }

    [Fact]
    public void ChangeCommentModerationPolicyOverride_SetsAndClearsOverride()
    {
        var blog = CreateBlog();

        blog.ChangeCommentModerationPolicyOverride(
            CommentModerationPolicy.AllComments,
            _baseTime.AddMinutes(1));

        Assert.Equal(
            CommentModerationPolicy.AllComments,
            blog.CommentModerationPolicyOverride);

        blog.ChangeCommentModerationPolicyOverride(
            null,
            _baseTime.AddMinutes(2));

        Assert.Null(blog.CommentModerationPolicyOverride);
        Assert.Equal(_baseTime.AddMinutes(2), blog.UpdatedAt);
    }

    [Fact]
    public void ChangeTaxonomy_CopiesTagSourceCollection()
    {
        var blog = CreateBlog();
        var firstTagId = BlogTagIdentity.New();
        var tagIds = new List<BlogTagIdentity> { firstTagId };

        blog.ChangeTaxonomy(
            null,
            tagIds,
            _baseTime.AddMinutes(1));
        tagIds.Add(BlogTagIdentity.New());

        Assert.Equal([firstTagId], blog.TagIds);
    }

    [Fact]
    public void ChangeCoAuthors_CopiesSourceCollection()
    {
        var blog = CreateBlog();
        var firstCoAuthor = CoAuthor.FromText("First");
        var coAuthors = new List<CoAuthor> { firstCoAuthor };

        blog.ChangeCoAuthors(
            coAuthors,
            _baseTime.AddMinutes(1));
        coAuthors.Add(CoAuthor.FromText("Second"));

        Assert.Equal([firstCoAuthor], blog.CoAuthors);
    }

    [Fact]
    public void UpdateDetails_WithInvalidTitle_DoesNotChangeBlog()
    {
        var blog = CreateBlog();

        Assert.Throws<ArgumentException>(
            () => blog.UpdateDetails(
                " ",
                "Changed summary",
                null,
                _baseTime.AddMinutes(1)));

        Assert.Equal("Blog title", blog.Title);
        Assert.Null(blog.Summary);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void ChangeTaxonomy_WithDuplicateTags_DoesNotChangeBlog()
    {
        var blog = CreateBlog();
        var tagId = BlogTagIdentity.New();

        Assert.Throws<ArgumentException>(
            () => blog.ChangeTaxonomy(
                CategoryIdentity.New(),
                [tagId, tagId],
                _baseTime.AddMinutes(1)));

        Assert.Null(blog.CategoryId);
        Assert.Empty(blog.TagIds);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void UpdateSearchMetadata_WithInvalidSeoTitle_DoesNotChangeBlog()
    {
        var blog = CreateBlog();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => blog.UpdateSearchMetadata(
                new string('x', Blog.MaximumSeoTitleLength + 1),
                null,
                null,
                _baseTime.AddMinutes(1)));

        Assert.Null(blog.SeoTitle);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void Edit_WhenBlogIsDeleted_ThrowsInvalidOperationException()
    {
        var blog = CreateBlog();
        blog.Delete(_baseTime.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            () => blog.UpdateDetails(
                "Changed",
                null,
                null,
                _baseTime.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(
            () => blog.ChangeVisibility(
                ContentVisibility.Private,
                _baseTime.AddMinutes(2)));
    }

    [Fact]
    public void AutosaveBody_WithEarlierTime_DoesNotChangeBlog()
    {
        var blocks = new ContentBlockCollection([]);
        var blog = CreateBlog(blocks);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => blog.AutosaveBody(
                CreateBlocks("Changed"),
                _baseTime.AddTicks(-1)));

        Assert.Same(blocks, blog.Blocks);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    private static Blog CreateBlog(
        ContentBlockCollection? blocks = null) =>
        Blog.Create(
            BlogIdentity.New(),
            UserIdentity.New(),
            new BlogSlug("000000001"),
            "Blog title",
            blocks ?? new ContentBlockCollection([]),
            null,
            null,
            ContentVisibility.Public,
            null,
            [],
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            pinned: false,
            featured: false,
            null,
            null,
            null,
            [],
            _baseTime);

    private static ContentBlockCollection CreateBlocks(string source) =>
        new(
        [
            new TextBlock(new ContentBody(source, ContentFormat.Markdown))
        ]);
}

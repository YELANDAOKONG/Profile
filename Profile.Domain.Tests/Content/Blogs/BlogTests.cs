using Profile.Domain.Content.Blocks;
using Profile.Domain.Content.Blogs;
using Profile.Domain.Content.Blogs.Value;
using Profile.Domain.Content.Categories.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Blogs;

public sealed class BlogTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithCompleteState_PreservesValuesAndStartsAsDraft()
    {
        var id = BlogIdentity.New();
        var authorId = UserIdentity.New();
        var slug = new BlogSlug("000000001");
        var blocks = new ContentBlockCollection([new DividerBlock()]);
        var featuredMedia = new MediaReference(MediaItemIdentity.New(), "Featured image");
        var categoryId = CategoryIdentity.New();
        BlogTagIdentity[] tagIds = [BlogTagIdentity.New(), BlogTagIdentity.New()];
        CoAuthor[] coAuthors =
        [
            CoAuthor.FromUser(UserIdentity.New()),
            CoAuthor.FromText("Guest Author")
        ];

        var blog = Blog.Create(
            id,
            authorId,
            slug,
            "Blog title",
            blocks,
            "Summary",
            featuredMedia,
            ContentVisibility.Followers,
            categoryId,
            tagIds,
            commentsAllowed: false,
            CommenterPolicy.FollowersOnly,
            pinned: true,
            featured: true,
            "SEO title",
            "SEO description",
            "https://example.com/canonical",
            coAuthors,
            _baseTime);

        Assert.Equal(id, blog.Id);
        Assert.Equal(authorId, blog.AuthorId);
        Assert.Equal(slug, blog.Slug);
        Assert.Equal("Blog title", blog.Title);
        Assert.Equal(blocks, blog.Blocks);
        Assert.Equal("Summary", blog.Summary);
        Assert.Equal(featuredMedia, blog.FeaturedMedia);
        Assert.Equal(ContentVisibility.Followers, blog.Visibility);
        Assert.Equal(categoryId, blog.CategoryId);
        Assert.Equal(tagIds, blog.TagIds);
        Assert.False(blog.CommentsAllowed);
        Assert.Equal(CommenterPolicy.FollowersOnly, blog.CommenterPolicy);
        Assert.True(blog.Pinned);
        Assert.True(blog.Featured);
        Assert.Equal("SEO title", blog.SeoTitle);
        Assert.Equal("SEO description", blog.SeoDescription);
        Assert.Equal("https://example.com/canonical", blog.CanonicalUrl);
        Assert.Equal(coAuthors, blog.CoAuthors);
        Assert.Equal(PublicationStatus.Draft, blog.Publication.Status);
        Assert.Null(blog.Deletion);
        Assert.Equal(_baseTime, blog.CreatedAt);
        Assert.Equal(_baseTime, blog.UpdatedAt);
    }

    [Fact]
    public void Create_WithEmptyBlocks_AllowsDraft()
    {
        var blog = CreateBlog();

        Assert.Empty(blog.Blocks.Blocks);
        Assert.Equal(PublicationStatus.Draft, blog.Publication.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceTitle_ThrowsArgumentException(string title)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateBlog(title: title));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullTitle_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlog(title: null!));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullIdentity_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlogWithRequiredValues(
                null!,
                UserIdentity.New(),
                new BlogSlug("000000001"),
                new ContentBlockCollection([]),
                [],
                []));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullAuthorIdentity_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlogWithRequiredValues(
                BlogIdentity.New(),
                null!,
                new BlogSlug("000000001"),
                new ContentBlockCollection([]),
                [],
                []));

        Assert.Equal("authorId", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullSlug_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlogWithRequiredValues(
                BlogIdentity.New(),
                UserIdentity.New(),
                null!,
                new ContentBlockCollection([]),
                [],
                []));

        Assert.Equal("slug", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullBlocks_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlogWithRequiredValues(
                BlogIdentity.New(),
                UserIdentity.New(),
                new BlogSlug("000000001"),
                null!,
                [],
                []));

        Assert.Equal("blocks", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullTagCollection_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlogWithRequiredValues(
                BlogIdentity.New(),
                UserIdentity.New(),
                new BlogSlug("000000001"),
                new ContentBlockCollection([]),
                null!,
                []));

        Assert.Equal("tagIds", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullCoAuthorCollection_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CreateBlogWithRequiredValues(
                BlogIdentity.New(),
                UserIdentity.New(),
                new BlogSlug("000000001"),
                new ContentBlockCollection([]),
                [],
                null!));

        Assert.Equal("coAuthors", exception.ParamName);
    }

    [Fact]
    public void Create_WithTitleAtMaximumLength_AcceptsValue()
    {
        var title = new string('a', Blog.MaximumTitleLength);

        var blog = CreateBlog(title: title);

        Assert.Equal(title, blog.Title);
    }

    [Fact]
    public void Create_WithTitleAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var title = new string('a', Blog.MaximumTitleLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(title: title));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Create_WithOptionalTextAtMaximumLengths_AcceptsValues()
    {
        var summary = new string('a', Blog.MaximumSummaryLength);
        var seoTitle = new string('b', Blog.MaximumSeoTitleLength);
        var seoDescription = new string('c', Blog.MaximumSeoDescriptionLength);

        var blog = CreateBlog(
            summary: summary,
            seoTitle: seoTitle,
            seoDescription: seoDescription);

        Assert.Equal(summary, blog.Summary);
        Assert.Equal(seoTitle, blog.SeoTitle);
        Assert.Equal(seoDescription, blog.SeoDescription);
    }

    [Fact]
    public void Create_WithSummaryAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var summary = new string('a', Blog.MaximumSummaryLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(summary: summary));

        Assert.Equal("summary", exception.ParamName);
    }

    [Fact]
    public void Create_WithSeoTitleAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var seoTitle = new string('a', Blog.MaximumSeoTitleLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(seoTitle: seoTitle));

        Assert.Equal("seoTitle", exception.ParamName);
    }

    [Fact]
    public void Create_WithSeoDescriptionAboveMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var seoDescription = new string('a', Blog.MaximumSeoDescriptionLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(seoDescription: seoDescription));

        Assert.Equal("seoDescription", exception.ParamName);
    }

    [Fact]
    public void Create_WithUndefinedVisibility_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(visibility: (ContentVisibility)999));

        Assert.Equal("visibility", exception.ParamName);
    }

    [Fact]
    public void Create_WithUndefinedCommenterPolicy_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(commenterPolicy: (CommenterPolicy)999));

        Assert.Equal("commenterPolicy", exception.ParamName);
    }

    [Fact]
    public void Create_WithMaximumTagCount_AcceptsValues()
    {
        var tagIds = Enumerable.Range(0, Blog.MaximumTagCount)
            .Select(static _ => BlogTagIdentity.New())
            .ToArray();

        var blog = CreateBlog(tagIds: tagIds);

        Assert.Equal(Blog.MaximumTagCount, blog.TagIds.Count);
    }

    [Fact]
    public void Create_WithTagCountAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        var tagIds = Enumerable.Range(0, Blog.MaximumTagCount + 1)
            .Select(static _ => BlogTagIdentity.New())
            .ToArray();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(tagIds: tagIds));

        Assert.Equal("tagIds", exception.ParamName);
    }

    [Fact]
    public void Create_WithDuplicateTagIds_ThrowsArgumentException()
    {
        var tagId = BlogTagIdentity.New();

        var exception = Assert.Throws<ArgumentException>(
            () => CreateBlog(tagIds: [tagId, tagId]));

        Assert.Equal("tagIds", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullTagItem_ThrowsArgumentException()
    {
        BlogTagIdentity[] tagIds = [BlogTagIdentity.New(), null!];

        var exception = Assert.Throws<ArgumentException>(
            () => CreateBlog(tagIds: tagIds));

        Assert.Equal("tagIds", exception.ParamName);
    }

    [Fact]
    public void Create_CopiesTagSourceCollection()
    {
        List<BlogTagIdentity> tagIds = [BlogTagIdentity.New()];
        var blog = CreateBlog(tagIds: tagIds);

        tagIds.Add(BlogTagIdentity.New());

        Assert.Single(blog.TagIds);
    }

    [Fact]
    public void Create_WithMaximumCoAuthorCount_AcceptsValues()
    {
        var coAuthors = Enumerable.Range(0, Blog.MaximumCoAuthorCount)
            .Select(index => CoAuthor.FromText($"Author {index}"))
            .ToArray();

        var blog = CreateBlog(coAuthors: coAuthors);

        Assert.Equal(Blog.MaximumCoAuthorCount, blog.CoAuthors.Count);
    }

    [Fact]
    public void Create_WithCoAuthorCountAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        var coAuthors = Enumerable.Range(0, Blog.MaximumCoAuthorCount + 1)
            .Select(index => CoAuthor.FromText($"Author {index}"))
            .ToArray();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBlog(coAuthors: coAuthors));

        Assert.Equal("coAuthors", exception.ParamName);
    }

    [Fact]
    public void Create_WithDuplicateCoAuthors_ThrowsArgumentException()
    {
        var coAuthor = CoAuthor.FromText("Guest Author");

        var exception = Assert.Throws<ArgumentException>(
            () => CreateBlog(coAuthors: [coAuthor, coAuthor]));

        Assert.Equal("coAuthors", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullCoAuthorItem_ThrowsArgumentException()
    {
        CoAuthor[] coAuthors = [CoAuthor.FromText("Guest Author"), null!];

        var exception = Assert.Throws<ArgumentException>(
            () => CreateBlog(coAuthors: coAuthors));

        Assert.Equal("coAuthors", exception.ParamName);
    }

    [Fact]
    public void Create_CopiesCoAuthorSourceCollection()
    {
        List<CoAuthor> coAuthors = [CoAuthor.FromText("Guest Author")];
        var blog = CreateBlog(coAuthors: coAuthors);

        coAuthors.Add(CoAuthor.FromText("Another Author"));

        Assert.Single(blog.CoAuthors);
    }

    [Fact]
    public void Reconstitute_WithPublishedDeletionState_PreservesValues()
    {
        var firstPublishedAt = _baseTime.AddMinutes(1);
        var lastPublishedAt = _baseTime.AddMinutes(2);
        var deletedAt = _baseTime.AddMinutes(3);
        var publication = Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            firstPublishedAt,
            lastPublishedAt);
        var deletion = ContentDeletion.Create(deletedAt);

        var blog = ReconstituteBlog(
            publication,
            deletion,
            _baseTime,
            deletedAt);

        Assert.Equal(publication, blog.Publication);
        Assert.Equal(deletion, blog.Deletion);
        Assert.Equal(_baseTime, blog.CreatedAt);
        Assert.Equal(deletedAt, blog.UpdatedAt);
    }

    [Fact]
    public void Reconstitute_WithUpdateBeforeCreation_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstituteBlog(
                Publication.CreateDraft(),
                null,
                _baseTime,
                _baseTime.AddTicks(-1)));

        Assert.Equal("updatedAt", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithNullPublication_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => ReconstituteBlog(
                null!,
                null,
                _baseTime,
                _baseTime));

        Assert.Equal("publication", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithPublishTimeBeforeCreation_ThrowsArgumentOutOfRangeException()
    {
        var publication = Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            _baseTime.AddTicks(-1),
            _baseTime);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstituteBlog(
                publication,
                null,
                _baseTime,
                _baseTime.AddMinutes(1)));

        Assert.Equal("publication", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithPublishTimeAfterUpdate_ThrowsArgumentOutOfRangeException()
    {
        var updatedAt = _baseTime.AddMinutes(1);
        var publication = Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            updatedAt,
            updatedAt.AddTicks(1));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstituteBlog(
                publication,
                null,
                _baseTime,
                updatedAt));

        Assert.Equal("publication", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithDeletionBeforeCreation_ThrowsArgumentOutOfRangeException()
    {
        var deletion = ContentDeletion.Create(_baseTime.AddTicks(-1));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstituteBlog(
                Publication.CreateDraft(),
                deletion,
                _baseTime,
                _baseTime));

        Assert.Equal("deletion", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithDeletionAfterUpdate_ThrowsArgumentOutOfRangeException()
    {
        var deletion = ContentDeletion.Create(_baseTime.AddMinutes(2));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReconstituteBlog(
                Publication.CreateDraft(),
                deletion,
                _baseTime,
                _baseTime.AddMinutes(1)));

        Assert.Equal("deletion", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithFutureScheduledTime_AllowsState()
    {
        var updatedAt = _baseTime.AddMinutes(1);
        var scheduledAt = updatedAt.AddDays(1);
        var publication = Publication.Reconstitute(
            PublicationStatus.Scheduled,
            scheduledAt,
            null,
            null);

        var blog = ReconstituteBlog(
            publication,
            null,
            _baseTime,
            updatedAt);

        Assert.Equal(scheduledAt, blog.Publication.ScheduledAt);
    }

    private static Blog CreateBlog(
        string title = "Blog title",
        string? summary = null,
        ContentVisibility visibility = ContentVisibility.Public,
        IEnumerable<BlogTagIdentity>? tagIds = null,
        CommenterPolicy commenterPolicy = CommenterPolicy.AllReaders,
        string? seoTitle = null,
        string? seoDescription = null,
        IEnumerable<CoAuthor>? coAuthors = null) =>
        Blog.Create(
            BlogIdentity.New(),
            UserIdentity.New(),
            new BlogSlug("000000001"),
            title,
            new ContentBlockCollection([]),
            summary,
            null,
            visibility,
            null,
            tagIds ?? [],
            commentsAllowed: true,
            commenterPolicy,
            pinned: false,
            featured: false,
            seoTitle,
            seoDescription,
            null,
            coAuthors ?? [],
            _baseTime);

    private static Blog CreateBlogWithRequiredValues(
        BlogIdentity id,
        UserIdentity authorId,
        BlogSlug slug,
        ContentBlockCollection blocks,
        IEnumerable<BlogTagIdentity> tagIds,
        IEnumerable<CoAuthor> coAuthors) =>
        Blog.Create(
            id,
            authorId,
            slug,
            "Blog title",
            blocks,
            null,
            null,
            ContentVisibility.Public,
            null,
            tagIds,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            pinned: false,
            featured: false,
            null,
            null,
            null,
            coAuthors,
            _baseTime);

    private static Blog ReconstituteBlog(
        Publication publication,
        ContentDeletion? deletion,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        Blog.Reconstitute(
            BlogIdentity.New(),
            UserIdentity.New(),
            new BlogSlug("000000001"),
            "Blog title",
            new ContentBlockCollection([]),
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
            publication,
            deletion,
            createdAt,
            updatedAt);
}

using Profile.Domain.Content.Value;

using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountSiteTests
{
    [Fact]
    public void Create_UsesDefaultPageSize()
    {
        var site = AccountSite.Create(
            UserIdentity.New(),
            "Site title",
            null,
            CreateAppearance(),
            CommentModerationPolicy.FirstComment,
            CreateOutputSettings());

        Assert.Equal(AccountSite.DefaultPageSize, site.PageSize);
        Assert.Null(site.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionAtMaximumLength_AcceptsValue()
    {
        var description = new string('a', AccountSite.MaximumDescriptionLength);

        var site = new AccountSite(
            UserIdentity.New(),
            "Site title",
            description,
            CreateAppearance(),
            AccountSite.MaximumPageSize,
            CommentModerationPolicy.AllComments,
            CreateOutputSettings());

        Assert.Equal(description, site.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException(string title)
    {
        Assert.Throws<ArgumentException>(
            () => AccountSite.Create(
                UserIdentity.New(),
                title,
                null,
                CreateAppearance(),
                CommentModerationPolicy.None,
                CreateOutputSettings()));
    }

    [Fact]
    public void Constructor_WithDescriptionAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AccountSite.Create(
                UserIdentity.New(),
                "Site title",
                new string('a', AccountSite.MaximumDescriptionLength + 1),
                CreateAppearance(),
                CommentModerationPolicy.None,
                CreateOutputSettings()));
    }

    [Theory]
    [InlineData(AccountSite.MinimumPageSize - 1)]
    [InlineData(AccountSite.MaximumPageSize + 1)]
    public void ChangePageSize_WithValueOutsideRange_ThrowsArgumentOutOfRangeException(
        int pageSize)
    {
        var site = CreateSite();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => site.ChangePageSize(pageSize));
    }

    [Fact]
    public void ConfigurationChanges_ReplaceIndependentSettings()
    {
        var site = CreateSite();
        var appearance = new SiteAppearance(
            new ThemeColor("#112233"),
            new ThemeColor("#44556677"),
            new FontFamily("serif"));
        var output = new SiteOutputSettings(false, false, false, false);

        site.UpdateDetails("Changed title", "Changed description");
        site.ChangeAppearance(appearance);
        site.ChangePageSize(AccountSite.MaximumPageSize);
        site.ChangeCommentModerationPolicy(CommentModerationPolicy.AllComments);
        site.ChangeOutputSettings(output);

        Assert.Equal("Changed title", site.Title);
        Assert.Equal("Changed description", site.Description);
        Assert.Equal(appearance, site.Appearance);
        Assert.Equal(AccountSite.MaximumPageSize, site.PageSize);
        Assert.Equal(CommentModerationPolicy.AllComments, site.CommentModerationPolicy);
        Assert.Equal(output, site.OutputSettings);
    }

    private static AccountSite CreateSite() =>
        AccountSite.Create(
            UserIdentity.New(),
            "Site title",
            null,
            CreateAppearance(),
            CommentModerationPolicy.FirstComment,
            CreateOutputSettings());

    private static SiteAppearance CreateAppearance() =>
        new(
            new ThemeColor("#336699"),
            new ThemeColor("#FFFFFF"),
            new FontFamily("sans-serif"));

    private static SiteOutputSettings CreateOutputSettings() =>
        new(true, true, true, true);
}

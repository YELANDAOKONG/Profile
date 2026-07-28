using Profile.Domain.Sites;
using Profile.Domain.Sites.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Sites;

public sealed class SiteTests
{
    [Fact]
    public void Constructor_WithNullableRootAccountSiteOwner_PreservesConfiguration()
    {
        var ownerId = UserIdentity.New();

        var site = new Site(
            SiteMode.Personal,
            RegistrationPolicy.Disabled,
            ownerId,
            null);

        Assert.Equal(ownerId, site.OwnerId);
        Assert.Null(site.RootAccountSiteOwnerId);
    }

    [Fact]
    public void SetRootAccountSiteOwner_WithAccount_ConfiguresDeploymentRoot()
    {
        var site = CreateSite();
        var rootOwnerId = UserIdentity.New();

        site.SetRootAccountSiteOwner(rootOwnerId);

        Assert.Equal(rootOwnerId, site.RootAccountSiteOwnerId);
    }

    [Fact]
    public void ChangeMode_ToCommunity_PreservesOwnerAndRootSiteConfiguration()
    {
        var ownerId = UserIdentity.New();
        var rootOwnerId = UserIdentity.New();
        var site = new Site(
            SiteMode.Personal,
            RegistrationPolicy.Disabled,
            ownerId,
            rootOwnerId);

        site.ChangeMode(SiteMode.Community);

        Assert.Equal(SiteMode.Community, site.Mode);
        Assert.Equal(ownerId, site.OwnerId);
        Assert.Equal(rootOwnerId, site.RootAccountSiteOwnerId);
    }

    [Fact]
    public void ChangeRegistrationPolicy_ToOpen_UpdatesIndependentlyFromMode()
    {
        var site = CreateSite();

        site.ChangeRegistrationPolicy(RegistrationPolicy.Open);

        Assert.Equal(SiteMode.Personal, site.Mode);
        Assert.Equal(RegistrationPolicy.Open, site.RegistrationPolicy);
    }

    [Fact]
    public void Constructor_WithUnsupportedMode_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Site(
                (SiteMode)999,
                RegistrationPolicy.Disabled,
                UserIdentity.New(),
                null));
    }

    [Fact]
    public void Constructor_WithUnsupportedRegistrationPolicy_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Site(
                SiteMode.Personal,
                (RegistrationPolicy)999,
                UserIdentity.New(),
                null));
    }

    private static Site CreateSite() =>
        new(
            SiteMode.Personal,
            RegistrationPolicy.Disabled,
            UserIdentity.New(),
            null);
}

using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountProfileTests
{
    [Fact]
    public void Constructor_WithCompleteProfile_PreservesValues()
    {
        var ownerId = UserIdentity.New();
        var avatar = new MediaReference(MediaItemIdentity.New(), "Avatar");
        var banner = new MediaReference(MediaItemIdentity.New(), null);
        var link = new PersonalLink("https://example.com");

        var profile = new AccountProfile(
            ownerId,
            "Display name",
            avatar,
            "Biography",
            "Taipei",
            link,
            banner);

        Assert.Equal(ownerId, profile.OwnerId);
        Assert.Equal("Display name", profile.Nickname);
        Assert.Equal(avatar, profile.Avatar);
        Assert.Equal("Biography", profile.Bio);
        Assert.Equal("Taipei", profile.Location);
        Assert.Equal(link, profile.PersonalLink);
        Assert.Equal(banner, profile.Banner);
    }

    [Fact]
    public void Constructor_WithValuesAtMaximumLength_AcceptsValues()
    {
        var profile = new AccountProfile(
            UserIdentity.New(),
            new string('a', AccountProfile.MaximumNicknameLength),
            null,
            new string('b', AccountProfile.MaximumBioLength),
            new string('c', AccountProfile.MaximumLocationLength),
            null,
            null);

        Assert.Equal(AccountProfile.MaximumNicknameLength, profile.Nickname.Length);
        Assert.Equal(AccountProfile.MaximumBioLength, profile.Bio?.Length);
        Assert.Equal(AccountProfile.MaximumLocationLength, profile.Location?.Length);
    }

    [Fact]
    public void Constructor_WithNicknameAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AccountProfile(
                UserIdentity.New(),
                new string('a', AccountProfile.MaximumNicknameLength + 1),
                null,
                null,
                null,
                null,
                null));
    }

    [Fact]
    public void Update_WithNewValues_ReplacesProfileData()
    {
        var profile = CreateProfile();
        var link = new PersonalLink("https://example.com/new");

        profile.Update("Changed", null, null, null, link, null);

        Assert.Equal("Changed", profile.Nickname);
        Assert.Null(profile.Avatar);
        Assert.Null(profile.Bio);
        Assert.Null(profile.Location);
        Assert.Equal(link, profile.PersonalLink);
        Assert.Null(profile.Banner);
    }

    private static AccountProfile CreateProfile() =>
        new(
            UserIdentity.New(),
            "Display name",
            null,
            null,
            null,
            null,
            null);
}

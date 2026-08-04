using Profile.Domain.Content.Value;
using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountSettingsTests
{
    [Fact]
    public void Create_UsesPublicContentVisibilityDefaults()
    {
        var settings = AccountSettings.Create(
            UserIdentity.New(),
            new LanguageTag("zh-TW"),
            new TimeZoneIdentifier("Asia/Taipei"),
            CreateNotifications(),
            false);

        Assert.Equal(ContentVisibility.Public, settings.DefaultBlogVisibility);
        Assert.Equal(ContentVisibility.Public, settings.DefaultPostVisibility);
        Assert.Equal(ContentVisibility.Public, settings.DefaultMomentVisibility);
    }

    [Fact]
    public void ChangeDefaultVisibilities_WithSupportedValues_UpdatesAllContentTypes()
    {
        var settings = CreateSettings();

        settings.ChangeDefaultVisibilities(
            ContentVisibility.Followers,
            ContentVisibility.MutualFollowers,
            ContentVisibility.Private);

        Assert.Equal(ContentVisibility.Followers, settings.DefaultBlogVisibility);
        Assert.Equal(ContentVisibility.MutualFollowers, settings.DefaultPostVisibility);
        Assert.Equal(ContentVisibility.Private, settings.DefaultMomentVisibility);
    }

    [Fact]
    public void ChangeDefaultVisibilities_WithUnsupportedValue_ThrowsArgumentOutOfRangeException()
    {
        var settings = CreateSettings();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => settings.ChangeDefaultVisibilities(
                (ContentVisibility)999,
                ContentVisibility.Public,
                ContentVisibility.Public));
    }

    [Fact]
    public void ChangeLocale_WithNewValues_UpdatesLanguageAndTimeZone()
    {
        var settings = CreateSettings();
        var language = new LanguageTag("en");
        var timeZone = new TimeZoneIdentifier("Etc/UTC");

        settings.ChangeLocale(language, timeZone);

        Assert.Equal(language, settings.Language);
        Assert.Equal(timeZone, settings.TimeZone);
    }

    [Fact]
    public void Preferences_CanBeChangedIndependently()
    {
        var settings = CreateSettings();
        var notifications = new EmailNotificationPreferences(
            false,
            false,
            false,
            false,
            false);

        settings.ChangeEmailNotifications(notifications);
        settings.SetFollowRequiresApproval(true);

        Assert.Equal(notifications, settings.EmailNotifications);
        Assert.True(settings.FollowRequiresApproval);
    }

    [Fact]
    public void DisableFollowApproval_WithoutDisposition_DefaultsToKeepPending()
    {
        var settings = CreateSettings(followRequiresApproval: true);

        var disposition = settings.SetFollowRequiresApproval(false);

        Assert.False(settings.FollowRequiresApproval);
        Assert.Equal(
            PendingFollowRequestDisposition.KeepPending,
            disposition);
    }

    [Fact]
    public void DisableFollowApproval_WithApproveAll_ReturnsSelectedDisposition()
    {
        var settings = CreateSettings(followRequiresApproval: true);

        var disposition = settings.SetFollowRequiresApproval(
            false,
            PendingFollowRequestDisposition.ApproveAll);

        Assert.False(settings.FollowRequiresApproval);
        Assert.Equal(
            PendingFollowRequestDisposition.ApproveAll,
            disposition);
    }

    [Fact]
    public void EnableFollowApproval_DoesNotReturnPendingDisposition()
    {
        var settings = CreateSettings();

        var disposition = settings.SetFollowRequiresApproval(true);

        Assert.True(settings.FollowRequiresApproval);
        Assert.Null(disposition);
    }

    [Fact]
    public void SetFollowApproval_ToCurrentValue_DoesNotReturnPendingDisposition()
    {
        var settings = CreateSettings();

        var disposition = settings.SetFollowRequiresApproval(false);

        Assert.False(settings.FollowRequiresApproval);
        Assert.Null(disposition);
    }

    [Fact]
    public void SetFollowApproval_WithUnsupportedDisposition_ThrowsArgumentOutOfRangeException()
    {
        var settings = CreateSettings(followRequiresApproval: true);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => settings.SetFollowRequiresApproval(
                false,
                (PendingFollowRequestDisposition)999));

        Assert.True(settings.FollowRequiresApproval);
    }

    private static AccountSettings CreateSettings(
        bool followRequiresApproval = false) =>
        AccountSettings.Create(
            UserIdentity.New(),
            new LanguageTag("zh-TW"),
            new TimeZoneIdentifier("Asia/Taipei"),
            CreateNotifications(),
            followRequiresApproval);

    private static EmailNotificationPreferences CreateNotifications() =>
        new(true, true, true, true, true);
}

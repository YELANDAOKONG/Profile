using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class AccountSettings
{
    public AccountSettings(
        UserIdentity ownerId,
        ContentVisibility defaultBlogVisibility,
        ContentVisibility defaultPostVisibility,
        ContentVisibility defaultMomentVisibility,
        LanguageTag language,
        TimeZoneIdentifier timeZone,
        EmailNotificationPreferences emailNotifications,
        bool followRequiresApproval)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(emailNotifications);

        ValidateVisibility(defaultBlogVisibility, nameof(defaultBlogVisibility));
        ValidateVisibility(defaultPostVisibility, nameof(defaultPostVisibility));
        ValidateVisibility(defaultMomentVisibility, nameof(defaultMomentVisibility));

        OwnerId = ownerId;
        DefaultBlogVisibility = defaultBlogVisibility;
        DefaultPostVisibility = defaultPostVisibility;
        DefaultMomentVisibility = defaultMomentVisibility;
        Language = language;
        TimeZone = timeZone;
        EmailNotifications = emailNotifications;
        FollowRequiresApproval = followRequiresApproval;
    }

    public UserIdentity OwnerId { get; }

    public ContentVisibility DefaultBlogVisibility { get; private set; }

    public ContentVisibility DefaultPostVisibility { get; private set; }

    public ContentVisibility DefaultMomentVisibility { get; private set; }

    public LanguageTag Language { get; private set; }

    public TimeZoneIdentifier TimeZone { get; private set; }

    public EmailNotificationPreferences EmailNotifications { get; private set; }

    public bool FollowRequiresApproval { get; private set; }

    public static AccountSettings Create(
        UserIdentity ownerId,
        LanguageTag language,
        TimeZoneIdentifier timeZone,
        EmailNotificationPreferences emailNotifications,
        bool followRequiresApproval) =>
        new(
            ownerId,
            ContentVisibility.Public,
            ContentVisibility.Public,
            ContentVisibility.Public,
            language,
            timeZone,
            emailNotifications,
            followRequiresApproval);

    public void ChangeDefaultVisibilities(
        ContentVisibility blog,
        ContentVisibility post,
        ContentVisibility moment)
    {
        ValidateVisibility(blog, nameof(blog));
        ValidateVisibility(post, nameof(post));
        ValidateVisibility(moment, nameof(moment));

        DefaultBlogVisibility = blog;
        DefaultPostVisibility = post;
        DefaultMomentVisibility = moment;
    }

    public void ChangeLocale(LanguageTag language, TimeZoneIdentifier timeZone)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(timeZone);

        Language = language;
        TimeZone = timeZone;
    }

    public void ChangeEmailNotifications(EmailNotificationPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        EmailNotifications = preferences;
    }

    public PendingFollowRequestDisposition? SetFollowRequiresApproval(
        bool value,
        PendingFollowRequestDisposition pendingRequestDisposition =
            PendingFollowRequestDisposition.KeepPending)
    {
        if (!Enum.IsDefined(pendingRequestDisposition))
        {
            throw new ArgumentOutOfRangeException(
                nameof(pendingRequestDisposition),
                pendingRequestDisposition,
                "Pending follow request disposition is not supported.");
        }

        if (FollowRequiresApproval == value)
        {
            return null;
        }

        FollowRequiresApproval = value;

        return value ? null : pendingRequestDisposition;
    }

    private static void ValidateVisibility(
        ContentVisibility visibility,
        string parameterName)
    {
        if (!Enum.IsDefined(visibility))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                visibility,
                "Content visibility is not supported.");
        }
    }
}

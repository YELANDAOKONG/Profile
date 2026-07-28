namespace Profile.Domain.Users;

public sealed record EmailNotificationPreferences(
    bool NewComments,
    bool CommentModeration,
    bool NewFollowers,
    bool Interactions,
    bool SystemNotifications);

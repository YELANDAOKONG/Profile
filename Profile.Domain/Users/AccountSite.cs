using Profile.Domain.Content.Value;

using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class AccountSite
{
    public const int MaximumTitleLength = 64;
    public const int MaximumDescriptionLength = 1_024;
    public const int MinimumPageSize = 1;
    public const int MaximumPageSize = 100;
    public const int DefaultPageSize = 20;

    public AccountSite(
        UserIdentity ownerId,
        string title,
        string? description,
        SiteAppearance appearance,
        int pageSize,
        CommentModerationPolicy commentModerationPolicy,
        SiteOutputSettings outputSettings)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(outputSettings);

        ValidateTitle(title);
        ValidateDescription(description);
        ValidatePageSize(pageSize);
        ValidateCommentModerationPolicy(commentModerationPolicy);

        OwnerId = ownerId;
        Title = title;
        Description = description;
        Appearance = appearance;
        PageSize = pageSize;
        CommentModerationPolicy = commentModerationPolicy;
        OutputSettings = outputSettings;
    }

    public UserIdentity OwnerId { get; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public SiteAppearance Appearance { get; private set; }

    public int PageSize { get; private set; }

    public CommentModerationPolicy CommentModerationPolicy { get; private set; }

    public SiteOutputSettings OutputSettings { get; private set; }

    public static AccountSite Create(
        UserIdentity ownerId,
        string title,
        string? description,
        SiteAppearance appearance,
        CommentModerationPolicy commentModerationPolicy,
        SiteOutputSettings outputSettings) =>
        new(
            ownerId,
            title,
            description,
            appearance,
            DefaultPageSize,
            commentModerationPolicy,
            outputSettings);

    public void UpdateDetails(string title, string? description)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title;
        Description = description;
    }

    public void ChangeAppearance(SiteAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(appearance);
        Appearance = appearance;
    }

    public void ChangePageSize(int pageSize)
    {
        ValidatePageSize(pageSize);
        PageSize = pageSize;
    }

    public void ChangeCommentModerationPolicy(CommentModerationPolicy policy)
    {
        ValidateCommentModerationPolicy(policy);
        CommentModerationPolicy = policy;
    }

    public void ChangeOutputSettings(SiteOutputSettings outputSettings)
    {
        ArgumentNullException.ThrowIfNull(outputSettings);
        OutputSettings = outputSettings;
    }

    private static void ValidateTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Site title cannot be empty or whitespace.",
                nameof(title));
        }

        if (title.Length > MaximumTitleLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(title),
                title.Length,
                $"Site title length cannot exceed {MaximumTitleLength} characters.");
        }
    }

    private static void ValidateDescription(string? description)
    {
        if (description is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Site description cannot be empty or whitespace.",
                nameof(description));
        }

        if (description.Length > MaximumDescriptionLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(description),
                description.Length,
                $"Site description length cannot exceed {MaximumDescriptionLength} characters.");
        }
    }

    private static void ValidatePageSize(int pageSize)
    {
        if (pageSize is < MinimumPageSize or > MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                $"Page size must be between {MinimumPageSize} and {MaximumPageSize}.");
        }
    }

    private static void ValidateCommentModerationPolicy(
        CommentModerationPolicy policy)
    {
        if (!Enum.IsDefined(policy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(policy),
                policy,
                "Comment moderation policy is not supported.");
        }
    }
}

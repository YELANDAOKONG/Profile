using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Content.Comments;

public static class CommentModerationPolicyResolver
{
    public static CommentModerationPolicy Resolve(
        CommentModerationPolicy siteDefault,
        CommentModerationPolicy? contentOverride)
    {
        ValidatePolicy(siteDefault, nameof(siteDefault));

        if (contentOverride is { } overridePolicy)
        {
            ValidatePolicy(overridePolicy, nameof(contentOverride));
            return overridePolicy;
        }

        return siteDefault;
    }

    public static CommentStatus DetermineInitialStatus(
        CommentModerationPolicy effectivePolicy,
        bool hasPreviouslyApprovedComment)
    {
        ValidatePolicy(effectivePolicy, nameof(effectivePolicy));

        return effectivePolicy switch
        {
            CommentModerationPolicy.None => CommentStatus.Approved,
            CommentModerationPolicy.FirstComment =>
                hasPreviouslyApprovedComment
                    ? CommentStatus.Approved
                    : CommentStatus.Pending,
            CommentModerationPolicy.AllComments => CommentStatus.Pending,
            _ => throw new ArgumentOutOfRangeException(
                nameof(effectivePolicy),
                effectivePolicy,
                "Comment moderation policy is not supported.")
        };
    }

    private static void ValidatePolicy(
        CommentModerationPolicy policy,
        string parameterName)
    {
        if (!Enum.IsDefined(policy))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                policy,
                "Comment moderation policy is not supported.");
        }
    }
}

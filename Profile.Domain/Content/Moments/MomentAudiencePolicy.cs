using Profile.Domain.Content.Value;
using Profile.Domain.Social;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Moments;

public static class MomentAudiencePolicy
{
    public static bool IsMember(
        Moment moment,
        UserIdentity? viewerId,
        bool viewerFollowsAuthor,
        bool authorFollowsViewer,
        bool isBlockedBetweenAccounts)
    {
        ArgumentNullException.ThrowIfNull(moment);

        var isInVisibilityAudience = ContentAudiencePolicy.IsMember(
            moment.Visibility,
            moment.AuthorId,
            viewerId,
            viewerFollowsAuthor,
            authorFollowsViewer,
            isBlockedBetweenAccounts);

        if (!isInVisibilityAudience || viewerId == moment.AuthorId)
        {
            return isInVisibilityAudience;
        }

        return moment.AudienceRestrictionMode switch
        {
            AudienceRestrictionMode.Blacklist =>
                viewerId is null ||
                !moment.AudienceAccountIds.Contains(viewerId),
            AudienceRestrictionMode.Whitelist =>
                viewerId is not null &&
                moment.AudienceAccountIds.Contains(viewerId),
            _ => throw new ArgumentOutOfRangeException(
                nameof(moment),
                moment.AudienceRestrictionMode,
                "Audience restriction mode is not supported.")
        };
    }
}

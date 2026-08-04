using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Social;

public static class ContentAudiencePolicy
{
    public static bool IsMember(
        ContentVisibility visibility,
        UserIdentity authorId,
        UserIdentity? viewerId,
        bool viewerFollowsAuthor,
        bool authorFollowsViewer,
        bool isBlocked)
    {
        ArgumentNullException.ThrowIfNull(authorId);

        if (!Enum.IsDefined(visibility))
        {
            throw new ArgumentOutOfRangeException(
                nameof(visibility),
                visibility,
                "Content visibility is not supported.");
        }

        if (viewerId == authorId)
        {
            return true;
        }

        return visibility switch
        {
            ContentVisibility.Public => true,
            ContentVisibility.Followers =>
                viewerId is not null && viewerFollowsAuthor && !isBlocked,
            ContentVisibility.MutualFollowers =>
                viewerId is not null && viewerFollowsAuthor &&
                authorFollowsViewer && !isBlocked,
            ContentVisibility.Private => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(visibility),
                visibility,
                "Content visibility is not supported.")
        };
    }
}

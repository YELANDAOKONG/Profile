using Profile.Domain.Content.Value;
using Profile.Domain.Social;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Posts;

public static class PostAudiencePolicy
{
    public static bool IsMember(
        Post post,
        UserIdentity? viewerId,
        bool viewerFollowsAuthor,
        bool authorFollowsViewer,
        bool isBlockedBetweenAccounts)
    {
        ArgumentNullException.ThrowIfNull(post);

        var isInVisibilityAudience = ContentAudiencePolicy.IsMember(
            post.Visibility,
            post.AuthorId,
            viewerId,
            viewerFollowsAuthor,
            authorFollowsViewer,
            isBlockedBetweenAccounts);

        if (!isInVisibilityAudience || viewerId == post.AuthorId)
        {
            return isInVisibilityAudience;
        }

        return post.AudienceRestrictionMode switch
        {
            AudienceRestrictionMode.Blacklist =>
                viewerId is null || !post.AudienceAccountIds.Contains(viewerId),
            AudienceRestrictionMode.Whitelist =>
                viewerId is not null &&
                post.AudienceAccountIds.Contains(viewerId),
            _ => throw new ArgumentOutOfRangeException(
                nameof(post),
                post.AudienceRestrictionMode,
                "Audience restriction mode is not supported.")
        };
    }
}

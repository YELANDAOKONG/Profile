using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Comments;

public static class CommentAudiencePolicy
{
    public static bool CanComment(
        UserIdentity contentAuthorId,
        UserIdentity commenterId,
        bool commentsAllowed,
        CommenterPolicy commenterPolicy,
        bool isInReadingAudience,
        bool commenterFollowsAuthor,
        bool authorFollowsCommenter,
        bool isBlockedBetweenAccounts)
    {
        ArgumentNullException.ThrowIfNull(contentAuthorId);
        ArgumentNullException.ThrowIfNull(commenterId);

        if (!Enum.IsDefined(commenterPolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(commenterPolicy),
                commenterPolicy,
                "Commenter policy is not supported.");
        }

        if (!commentsAllowed ||
            !isInReadingAudience ||
            isBlockedBetweenAccounts)
        {
            return false;
        }

        if (commenterId == contentAuthorId)
        {
            return true;
        }

        return commenterPolicy switch
        {
            CommenterPolicy.AllReaders => true,
            CommenterPolicy.FollowersOnly => commenterFollowsAuthor,
            CommenterPolicy.MutualFollowersOnly =>
                commenterFollowsAuthor && authorFollowsCommenter,
            CommenterPolicy.AuthorOnly => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(commenterPolicy),
                commenterPolicy,
                "Commenter policy is not supported.")
        };
    }
}

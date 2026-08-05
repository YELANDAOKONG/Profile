using Profile.Domain.Content.Comments.Value;

namespace Profile.Domain.Content.Likes;

public static class LikeEligibilityPolicy
{
    public static void EnsureCanLikeContent(
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts)
    {
        if (isBlockedBetweenAccounts)
        {
            throw new InvalidOperationException(
                "An account cannot like content while either account blocks the other.");
        }

        if (!isTargetActiveAndPublished)
        {
            throw new ArgumentException(
                "Only active, published content can be liked.",
                nameof(isTargetActiveAndPublished));
        }

        if (!isInReadingAudience)
        {
            throw new InvalidOperationException(
                "An account cannot like content outside its reading audience.");
        }
    }

    public static void EnsureCanLikeComment(
        CommentStatus commentStatus,
        bool isHostActiveAndPublished,
        bool isInHostReadingAudience,
        bool isBlockedBetweenAccounts)
    {
        if (!Enum.IsDefined(commentStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(commentStatus),
                commentStatus,
                "Comment status is not supported.");
        }

        EnsureCanLikeContent(
            isHostActiveAndPublished,
            isInHostReadingAudience,
            isBlockedBetweenAccounts);

        if (commentStatus is not CommentStatus.Approved)
        {
            throw new ArgumentException(
                "Only an approved comment can be liked.",
                nameof(commentStatus));
        }
    }
}

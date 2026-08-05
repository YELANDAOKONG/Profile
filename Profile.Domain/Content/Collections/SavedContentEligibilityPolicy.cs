namespace Profile.Domain.Content.Collections;

public static class SavedContentEligibilityPolicy
{
    public static void EnsureCanSave(
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts)
    {
        if (isBlockedBetweenAccounts)
        {
            throw new InvalidOperationException(
                "An account cannot save content while either account blocks the other.");
        }

        if (!isTargetActiveAndPublished)
        {
            throw new ArgumentException(
                "Only active, published content can be saved.",
                nameof(isTargetActiveAndPublished));
        }

        if (!isInReadingAudience)
        {
            throw new InvalidOperationException(
                "An account cannot save content outside its reading audience.");
        }
    }
}

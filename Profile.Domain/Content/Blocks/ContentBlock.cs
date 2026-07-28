namespace Profile.Domain.Content.Blocks;

public abstract record ContentBlock
{
    public const int MaximumTextLength = 2_097_152;

    private protected static void EnsureTextWithinLimit(
        string text,
        string parameterName)
    {
        if (text.Length > MaximumTextLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                text.Length,
                $"Textual content block text cannot exceed {MaximumTextLength} characters.");
        }
    }
}

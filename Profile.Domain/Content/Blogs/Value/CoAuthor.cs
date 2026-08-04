using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Blogs.Value;

public sealed record CoAuthor
{
    public const int MaximumTextLength = 64;

    private CoAuthor(UserIdentity? userId, string? text)
    {
        UserId = userId;
        Text = text;
    }

    public UserIdentity? UserId { get; }

    public string? Text { get; }

    public static CoAuthor FromUser(UserIdentity userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        return new CoAuthor(userId, null);
    }

    public static CoAuthor FromText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length > MaximumTextLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(text),
                text.Length,
                $"Co-author text length cannot exceed {MaximumTextLength} characters.");
        }

        return new CoAuthor(null, text);
    }
}

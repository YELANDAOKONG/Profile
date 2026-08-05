namespace Profile.Domain.Content.Bookmarks.Value;

public sealed record PostBookmarkFolderIdentity
{
    public PostBookmarkFolderIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Post bookmark folder identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PostBookmarkFolderIdentity New() => new(Guid.NewGuid());
}

namespace Profile.Domain.Content.Favorites.Value;

public sealed record BlogFavoriteFolderIdentity
{
    public BlogFavoriteFolderIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Blog favorite folder identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static BlogFavoriteFolderIdentity New() => new(Guid.NewGuid());
}

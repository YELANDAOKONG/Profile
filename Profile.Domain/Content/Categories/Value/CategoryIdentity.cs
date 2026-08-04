namespace Profile.Domain.Content.Categories.Value;

public sealed record CategoryIdentity
{
    public CategoryIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Category identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static CategoryIdentity New() => new(Guid.NewGuid());
}

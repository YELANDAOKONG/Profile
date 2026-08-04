namespace Profile.Domain.Content.Tags.Value;

public sealed record BlogTagIdentity
{
    public BlogTagIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Blog tag identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static BlogTagIdentity New() => new(Guid.NewGuid());
}

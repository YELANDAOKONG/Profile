namespace Profile.Domain.Content.Tags.Value;

public sealed record PostTagIdentity
{
    public PostTagIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Post tag identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PostTagIdentity New() => new(Guid.NewGuid());
}

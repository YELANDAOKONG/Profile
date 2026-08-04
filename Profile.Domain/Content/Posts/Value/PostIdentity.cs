namespace Profile.Domain.Content.Posts.Value;

public sealed record PostIdentity
{
    public PostIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Post identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PostIdentity New() => new(Guid.NewGuid());
}

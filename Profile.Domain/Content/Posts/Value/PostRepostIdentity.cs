namespace Profile.Domain.Content.Posts.Value;

public sealed record PostRepostIdentity
{
    public PostRepostIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Post repost identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PostRepostIdentity New() => new(Guid.NewGuid());
}

namespace Profile.Domain.Content.Moments.Value;

public sealed record MomentRepostIdentity
{
    public MomentRepostIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Moment repost identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static MomentRepostIdentity New() => new(Guid.NewGuid());
}

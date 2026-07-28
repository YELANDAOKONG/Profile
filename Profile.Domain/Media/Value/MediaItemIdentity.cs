namespace Profile.Domain.Media.Value;

public sealed record MediaItemIdentity
{
    public MediaItemIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Media item identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static MediaItemIdentity New() => new(Guid.NewGuid());
}

namespace Profile.Domain.Content.Tags.Value;

public sealed record MomentTagIdentity
{
    public MomentTagIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Moment tag identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static MomentTagIdentity New() => new(Guid.NewGuid());
}

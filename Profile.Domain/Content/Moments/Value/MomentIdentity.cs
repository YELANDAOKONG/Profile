namespace Profile.Domain.Content.Moments.Value;

public sealed record MomentIdentity
{
    public MomentIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Moment identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static MomentIdentity New() => new(Guid.NewGuid());
}

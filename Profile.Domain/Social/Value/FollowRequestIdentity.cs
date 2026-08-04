namespace Profile.Domain.Social.Value;

public sealed record FollowRequestIdentity
{
    public FollowRequestIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Follow request identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static FollowRequestIdentity New() => new(Guid.NewGuid());
}

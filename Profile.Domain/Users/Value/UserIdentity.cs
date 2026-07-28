namespace Profile.Domain.Users.Value;

public sealed record UserIdentity
{
    public UserIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "User identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static UserIdentity New() => new(Guid.NewGuid());
}

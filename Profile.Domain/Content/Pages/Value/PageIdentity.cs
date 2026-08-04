namespace Profile.Domain.Content.Pages.Value;

public sealed record PageIdentity
{
    public PageIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Page identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PageIdentity New() => new(Guid.NewGuid());
}

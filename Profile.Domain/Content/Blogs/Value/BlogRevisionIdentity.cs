namespace Profile.Domain.Content.Blogs.Value;

public sealed record BlogRevisionIdentity
{
    public BlogRevisionIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Blog revision identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static BlogRevisionIdentity New() => new(Guid.NewGuid());
}

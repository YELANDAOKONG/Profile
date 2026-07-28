namespace Profile.Domain.Content.Blogs.Value;

public sealed record BlogIdentity
{
    public BlogIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Blog identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static BlogIdentity New() => new(Guid.NewGuid());
}

namespace Profile.Domain.Content.Comments.Value;

public sealed record BlogCommentIdentity
{
    public BlogCommentIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Blog comment identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static BlogCommentIdentity New() => new(Guid.NewGuid());
}

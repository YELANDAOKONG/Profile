namespace Profile.Domain.Content.Comments.Value;

public sealed record PageCommentIdentity
{
    public PageCommentIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Page comment identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PageCommentIdentity New() => new(Guid.NewGuid());
}

namespace Profile.Domain.Content.Comments.Value;

public sealed record PostCommentIdentity
{
    public PostCommentIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Post comment identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PostCommentIdentity New() => new(Guid.NewGuid());
}

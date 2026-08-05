namespace Profile.Domain.Content.Comments.Value;

public sealed record MomentCommentIdentity
{
    public MomentCommentIdentity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Moment comment identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static MomentCommentIdentity New() => new(Guid.NewGuid());
}

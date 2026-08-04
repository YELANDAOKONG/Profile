using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Moments;

public sealed record MomentRepost
{
    public MomentRepost(
        MomentRepostIdentity id,
        UserIdentity reposterId,
        MomentIdentity momentId,
        DateTimeOffset repostedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(reposterId);
        ArgumentNullException.ThrowIfNull(momentId);

        Id = id;
        ReposterId = reposterId;
        MomentId = momentId;
        RepostedAt = repostedAt;
    }

    public MomentRepostIdentity Id { get; }

    public UserIdentity ReposterId { get; }

    public MomentIdentity MomentId { get; }

    public DateTimeOffset RepostedAt { get; }

    public static MomentRepost Create(
        UserIdentity reposterId,
        Moment moment,
        bool isBlockedBetweenAuthors,
        DateTimeOffset repostedAt)
    {
        ArgumentNullException.ThrowIfNull(reposterId);
        ArgumentNullException.ThrowIfNull(moment);

        moment.EnsureCanBeShared(
            isBlockedBetweenAuthors,
            nameof(moment));

        return new MomentRepost(
            MomentRepostIdentity.New(),
            reposterId,
            moment.Id,
            repostedAt);
    }
}

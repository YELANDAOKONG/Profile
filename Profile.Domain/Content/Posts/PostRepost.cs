using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Content.Posts;

public sealed record PostRepost
{
    public PostRepost(
        PostRepostIdentity id,
        UserIdentity reposterId,
        PostIdentity postId,
        DateTimeOffset repostedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(reposterId);
        ArgumentNullException.ThrowIfNull(postId);

        Id = id;
        ReposterId = reposterId;
        PostId = postId;
        RepostedAt = repostedAt;
    }

    public PostRepostIdentity Id { get; }

    public UserIdentity ReposterId { get; }

    public PostIdentity PostId { get; }

    public DateTimeOffset RepostedAt { get; }

    public static PostRepost Create(
        UserIdentity reposterId,
        Post post,
        bool isBlockedBetweenAuthors,
        DateTimeOffset repostedAt)
    {
        ArgumentNullException.ThrowIfNull(reposterId);
        ArgumentNullException.ThrowIfNull(post);

        post.EnsureCanBeShared(
            isBlockedBetweenAuthors,
            nameof(post));

        return new PostRepost(
            PostRepostIdentity.New(),
            reposterId,
            post.Id,
            repostedAt);
    }
}

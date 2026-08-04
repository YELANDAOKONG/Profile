using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Posts;

internal static class PostTestFactory
{
    public static readonly DateTimeOffset BaseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static Post CreatePost() =>
        ReconstitutePost();

    public static Post ReconstitutePost(
        Publication? publication = null,
        ContentDeletion? deletion = null,
        ContentVisibility visibility = ContentVisibility.Public,
        AudienceRestrictionMode audienceRestrictionMode =
            AudienceRestrictionMode.Blacklist,
        IEnumerable<UserIdentity>? audienceAccountIds = null,
        UserIdentity? authorId = null,
        PostIdentity? quotedPostId = null,
        DateTimeOffset? updatedAt = null) =>
        Post.Reconstitute(
            PostIdentity.New(),
            authorId ?? UserIdentity.New(),
            CreateBody(),
            [],
            visibility,
            audienceRestrictionMode,
            audienceAccountIds ?? [],
            quotedPostId,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            [],
            publication ?? Publication.CreateDraft(),
            deletion,
            BaseTime,
            updatedAt ?? BaseTime);

    public static ContentBody CreateBody(string source = "Post body") =>
        new(source, ContentFormat.PlainText);

    public static MediaReference CreateMedia(string? altText = null) =>
        new(MediaItemIdentity.New(), altText);
}

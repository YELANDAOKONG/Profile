using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Moments;

internal static class MomentTestFactory
{
    public static readonly DateTimeOffset BaseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static Moment CreateMoment() =>
        ReconstituteMoment();

    public static Moment ReconstituteMoment(
        Publication? publication = null,
        ContentDeletion? deletion = null,
        ContentVisibility visibility = ContentVisibility.Public,
        AudienceRestrictionMode audienceRestrictionMode =
            AudienceRestrictionMode.Blacklist,
        IEnumerable<UserIdentity>? audienceAccountIds = null,
        UserIdentity? authorId = null,
        MomentLocation? location = null,
        MomentIdentity? quotedMomentId = null,
        DateTimeOffset? updatedAt = null) =>
        Moment.Reconstitute(
            MomentIdentity.New(),
            authorId ?? UserIdentity.New(),
            CreateBody(),
            [],
            visibility,
            audienceRestrictionMode,
            audienceAccountIds ?? [],
            location,
            quotedMomentId,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            [],
            publication ?? Publication.CreateDraft(),
            deletion,
            BaseTime,
            updatedAt ?? BaseTime);

    public static ContentBody CreateBody(string source = "Moment body") =>
        new(source, ContentFormat.PlainText);

    public static MediaReference CreateMedia(string? altText = null) =>
        new(MediaItemIdentity.New(), altText);
}

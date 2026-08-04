using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Media.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Moments;

public sealed class MomentTests
{
    [Fact]
    public void Create_WithCompleteState_CreatesDraftAndPreservesValues()
    {
        var id = MomentIdentity.New();
        var authorId = UserIdentity.New();
        var body = MomentTestFactory.CreateBody();
        var media = MomentTestFactory.CreateMedia("Description");
        var audienceAccountId = UserIdentity.New();
        var tagId = MomentTagIdentity.New();

        var moment = Moment.Create(
            id,
            authorId,
            body,
            [media],
            ContentVisibility.Followers,
            AudienceRestrictionMode.Whitelist,
            [audienceAccountId],
            new MomentLocation(25.033m, 121.5654m, "Taipei 101"),
            commentsAllowed: false,
            CommenterPolicy.MutualFollowersOnly,
            [tagId],
            MomentTestFactory.BaseTime);

        Assert.Equal(id, moment.Id);
        Assert.Equal(authorId, moment.AuthorId);
        Assert.Same(body, moment.Body);
        Assert.Equal([media], moment.Media);
        Assert.Equal(ContentVisibility.Followers, moment.Visibility);
        Assert.Equal(
            AudienceRestrictionMode.Whitelist,
            moment.AudienceRestrictionMode);
        Assert.Equal([audienceAccountId], moment.AudienceAccountIds);
        Assert.Equal(
            new MomentLocation(25.033m, 121.5654m, "Taipei 101"),
            moment.Location);
        Assert.Null(moment.QuotedMomentId);
        Assert.False(moment.CommentsAllowed);
        Assert.Equal(CommenterPolicy.MutualFollowersOnly, moment.CommenterPolicy);
        Assert.Equal([tagId], moment.TagIds);
        Assert.Equal(PublicationStatus.Draft, moment.Publication.Status);
        Assert.Null(moment.Deletion);
        Assert.Equal(MomentTestFactory.BaseTime, moment.CreatedAt);
        Assert.Equal(MomentTestFactory.BaseTime, moment.UpdatedAt);
    }

    [Fact]
    public void Create_WithBodyOnly_AllowsDraft()
    {
        var moment = CreateMoment(MomentTestFactory.CreateBody(), []);

        Assert.NotNull(moment.Body);
        Assert.Empty(moment.Media);
    }

    [Fact]
    public void Create_WithMediaOnly_AllowsDraft()
    {
        var moment = CreateMoment(null, [MomentTestFactory.CreateMedia()]);

        Assert.Null(moment.Body);
        Assert.Single(moment.Media);
    }

    [Fact]
    public void Create_WithoutBodyOrMedia_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreateMoment(null, []));
    }

    [Fact]
    public void Create_WithLocationOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateMoment(
                null,
                [],
                location: new MomentLocation(25.033m, 121.5654m)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void Create_WithEmptyOrWhitespaceBody_ThrowsArgumentException(
        string source)
    {
        Assert.Throws<ArgumentException>(
            () => CreateMoment(MomentTestFactory.CreateBody(source), []));
    }

    [Fact]
    public void Create_WithMaximumBodyLength_AllowsValue()
    {
        var body = MomentTestFactory.CreateBody(
            new string('a', Moment.MaximumBodyLength));

        var moment = CreateMoment(body, []);

        Assert.Equal(Moment.MaximumBodyLength, moment.Body?.Source.Length);
    }

    [Fact]
    public void Create_WithBodyOverMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var body = MomentTestFactory.CreateBody(
            new string('a', Moment.MaximumBodyLength + 1));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateMoment(body, []));
    }

    [Fact]
    public void Create_WithMaximumMediaCount_AllowsValue()
    {
        var media = Enumerable.Range(0, Moment.MaximumMediaCount)
            .Select(_ => MomentTestFactory.CreateMedia())
            .ToArray();

        var moment = CreateMoment(null, media);

        Assert.Equal(Moment.MaximumMediaCount, moment.Media.Count);
    }

    [Fact]
    public void Create_WithTooManyMedia_ThrowsArgumentOutOfRangeException()
    {
        var media = Enumerable.Range(0, Moment.MaximumMediaCount + 1)
            .Select(_ => MomentTestFactory.CreateMedia())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateMoment(null, media));
    }

    [Fact]
    public void Create_WithNullMediaItem_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => CreateMoment(null, [MomentTestFactory.CreateMedia(), null!]));
    }

    [Fact]
    public void Create_WithDuplicateMediaIdentity_ThrowsArgumentException()
    {
        var mediaId = MediaItemIdentity.New();
        MediaReference[] media =
        [
            new(mediaId, "First use"),
            new(mediaId, "Second use")
        ];

        Assert.Throws<ArgumentException>(() => CreateMoment(null, media));
    }

    [Fact]
    public void Create_CopiesMediaCollection()
    {
        List<MediaReference> media = [MomentTestFactory.CreateMedia()];
        var moment = CreateMoment(null, media);

        media.Add(MomentTestFactory.CreateMedia());

        Assert.Single(moment.Media);
    }

    [Fact]
    public void Create_WithMaximumAudienceAccountCount_AllowsValue()
    {
        var audienceAccountIds = Enumerable
            .Range(0, Moment.MaximumAudienceAccountCount)
            .Select(_ => UserIdentity.New())
            .ToArray();

        var moment = CreateMoment(audienceAccountIds: audienceAccountIds);

        Assert.Equal(
            Moment.MaximumAudienceAccountCount,
            moment.AudienceAccountIds.Count);
    }

    [Fact]
    public void Create_WithTooManyAudienceAccounts_ThrowsArgumentOutOfRangeException()
    {
        var audienceAccountIds = Enumerable
            .Range(0, Moment.MaximumAudienceAccountCount + 1)
            .Select(_ => UserIdentity.New())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateMoment(audienceAccountIds: audienceAccountIds));
    }

    [Fact]
    public void Create_WithDuplicateAudienceAccount_ThrowsArgumentException()
    {
        var accountId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreateMoment(audienceAccountIds: [accountId, accountId]));
    }

    [Fact]
    public void Create_WithAuthorInAudienceAccounts_ThrowsArgumentException()
    {
        var authorId = UserIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreateMoment(
                authorId: authorId,
                audienceAccountIds: [authorId]));
    }

    [Fact]
    public void Create_WithMaximumTagCount_AllowsValue()
    {
        var tagIds = Enumerable.Range(0, Moment.MaximumTagCount)
            .Select(_ => MomentTagIdentity.New())
            .ToArray();

        var moment = CreateMoment(tagIds: tagIds);

        Assert.Equal(Moment.MaximumTagCount, moment.TagIds.Count);
    }

    [Fact]
    public void Create_WithTooManyTags_ThrowsArgumentOutOfRangeException()
    {
        var tagIds = Enumerable.Range(0, Moment.MaximumTagCount + 1)
            .Select(_ => MomentTagIdentity.New())
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateMoment(tagIds: tagIds));
    }

    [Fact]
    public void Create_WithDuplicateTag_ThrowsArgumentException()
    {
        var tagId = MomentTagIdentity.New();

        Assert.Throws<ArgumentException>(
            () => CreateMoment(tagIds: [tagId, tagId]));
    }

    [Fact]
    public void Reconstitute_WithQuotedIdentityEqualToOwn_ThrowsArgumentException()
    {
        var id = MomentIdentity.New();

        Assert.Throws<ArgumentException>(
            () => Moment.Reconstitute(
                id,
                UserIdentity.New(),
                MomentTestFactory.CreateBody(),
                [],
                ContentVisibility.Public,
                AudienceRestrictionMode.Blacklist,
                [],
                null,
                id,
                commentsAllowed: true,
                CommenterPolicy.AllReaders,
                [],
                Publication.CreateDraft(),
                null,
                MomentTestFactory.BaseTime,
                MomentTestFactory.BaseTime));
    }

    [Theory]
    [InlineData(999, 0, 0)]
    [InlineData(0, 999, 0)]
    [InlineData(0, 0, 999)]
    public void Create_WithUnsupportedEnum_ThrowsArgumentOutOfRangeException(
        int visibility,
        int mode,
        int commenterPolicy)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateMoment(
                visibility: (ContentVisibility)visibility,
                audienceRestrictionMode: (AudienceRestrictionMode)mode,
                commenterPolicy: (CommenterPolicy)commenterPolicy));
    }

    private static Moment CreateMoment(
        ContentBody? body = null,
        IEnumerable<MediaReference>? media = null,
        UserIdentity? authorId = null,
        ContentVisibility visibility = ContentVisibility.Public,
        AudienceRestrictionMode audienceRestrictionMode =
            AudienceRestrictionMode.Blacklist,
        IEnumerable<UserIdentity>? audienceAccountIds = null,
        MomentLocation? location = null,
        CommenterPolicy commenterPolicy = CommenterPolicy.AllReaders,
        IEnumerable<MomentTagIdentity>? tagIds = null) =>
        Moment.Create(
            MomentIdentity.New(),
            authorId ?? UserIdentity.New(),
            body ?? (media is null ? MomentTestFactory.CreateBody() : null),
            media ?? [],
            visibility,
            audienceRestrictionMode,
            audienceAccountIds ?? [],
            location,
            commentsAllowed: true,
            commenterPolicy,
            tagIds ?? [],
            MomentTestFactory.BaseTime);
}

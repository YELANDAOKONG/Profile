using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Content.Tags.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Moments;

public sealed class MomentEditingTests
{
    [Fact]
    public void UpdateContent_FromDraft_ReplacesBodyAndMedia()
    {
        var moment = MomentTestFactory.CreateMoment();
        var body = MomentTestFactory.CreateBody("Changed body");
        var media = MomentTestFactory.CreateMedia();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        var location = new MomentLocation(25.033m, 121.5654m, "Taipei 101");

        moment.UpdateContent(body, [media], location, changedAt);

        Assert.Same(body, moment.Body);
        Assert.Equal([media], moment.Media);
        Assert.Equal(location, moment.Location);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_ToMediaOnly_AllowsValue()
    {
        var moment = MomentTestFactory.CreateMoment();

        moment.UpdateContent(
            null,
            [MomentTestFactory.CreateMedia()],
            null,
            MomentTestFactory.BaseTime.AddMinutes(1));

        Assert.Null(moment.Body);
        Assert.Single(moment.Media);
    }

    [Fact]
    public void UpdateContent_ToLocationOnly_ThrowsAndPreservesContent()
    {
        var moment = MomentTestFactory.CreateMoment();
        var body = moment.Body;

        Assert.Throws<ArgumentException>(
            () => moment.UpdateContent(
                null,
                [],
                new MomentLocation(25.033m, 121.5654m),
                MomentTestFactory.BaseTime.AddMinutes(1)));

        Assert.Same(body, moment.Body);
        Assert.Empty(moment.Media);
        Assert.Equal(MomentTestFactory.BaseTime, moment.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_WhilePublished_ThrowsInvalidOperationException()
    {
        var moment = CreatePublishedMoment();

        Assert.Throws<InvalidOperationException>(
            () => moment.UpdateContent(
                MomentTestFactory.CreateBody("Changed"),
                [],
                null,
                MomentTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void UpdateContent_AfterUnpublishing_AllowsValue()
    {
        var moment = CreatePublishedMoment();
        moment.UnpublishToDraft(MomentTestFactory.BaseTime.AddMinutes(1));
        var body = MomentTestFactory.CreateBody("Changed");

        moment.UpdateContent(
            body,
            [],
            null,
            MomentTestFactory.BaseTime.AddMinutes(2));

        Assert.Same(body, moment.Body);
        Assert.Equal(MomentTestFactory.BaseTime.AddMinutes(2), moment.UpdatedAt);
    }

    [Fact]
    public void ChangeVisibility_WhilePublished_AllowsValue()
    {
        var moment = CreatePublishedMoment();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.ChangeVisibility(ContentVisibility.Followers, changedAt);

        Assert.Equal(ContentVisibility.Followers, moment.Visibility);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void ChangeAudienceRestriction_WhilePublished_AllowsValue()
    {
        var moment = CreatePublishedMoment();
        var accountId = UserIdentity.New();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.ChangeAudienceRestriction(
            AudienceRestrictionMode.Whitelist,
            [accountId],
            changedAt);

        Assert.Equal(
            AudienceRestrictionMode.Whitelist,
            moment.AudienceRestrictionMode);
        Assert.Equal([accountId], moment.AudienceAccountIds);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscussion_WhilePublished_AllowsValue()
    {
        var moment = CreatePublishedMoment();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.ChangeDiscussion(
            commentsAllowed: false,
            CommenterPolicy.AuthorOnly,
            changedAt);

        Assert.False(moment.CommentsAllowed);
        Assert.Equal(CommenterPolicy.AuthorOnly, moment.CommenterPolicy);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void ChangeTags_WhilePublished_AllowsValue()
    {
        var moment = CreatePublishedMoment();
        var tagId = MomentTagIdentity.New();
        var changedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        moment.ChangeTags([tagId], changedAt);

        Assert.Equal([tagId], moment.TagIds);
        Assert.Equal(changedAt, moment.UpdatedAt);
    }

    [Fact]
    public void ChangeAudienceRestriction_WithAuthor_ThrowsAndPreservesState()
    {
        var moment = MomentTestFactory.CreateMoment();

        Assert.Throws<ArgumentException>(
            () => moment.ChangeAudienceRestriction(
                AudienceRestrictionMode.Whitelist,
                [moment.AuthorId],
                MomentTestFactory.BaseTime.AddMinutes(1)));

        Assert.Equal(
            AudienceRestrictionMode.Blacklist,
            moment.AudienceRestrictionMode);
        Assert.Empty(moment.AudienceAccountIds);
        Assert.Equal(MomentTestFactory.BaseTime, moment.UpdatedAt);
    }

    [Theory]
    [InlineData(nameof(Moment.UpdateContent))]
    [InlineData(nameof(Moment.ChangeVisibility))]
    [InlineData(nameof(Moment.ChangeAudienceRestriction))]
    [InlineData(nameof(Moment.ChangeDiscussion))]
    [InlineData(nameof(Moment.ChangeTags))]
    public void ChangeOperation_WhenDeleted_ThrowsInvalidOperationException(
        string operation)
    {
        var deletedAt = MomentTestFactory.BaseTime.AddMinutes(1);
        var moment = MomentTestFactory.ReconstituteMoment(
            deletion: ContentDeletion.Create(deletedAt),
            updatedAt: deletedAt);

        Assert.Throws<InvalidOperationException>(
            () => InvokeChange(moment, operation, deletedAt));
    }

    [Theory]
    [InlineData(nameof(Moment.UpdateContent))]
    [InlineData(nameof(Moment.ChangeVisibility))]
    [InlineData(nameof(Moment.ChangeAudienceRestriction))]
    [InlineData(nameof(Moment.ChangeDiscussion))]
    [InlineData(nameof(Moment.ChangeTags))]
    public void ChangeOperation_WithEarlierTime_ThrowsArgumentOutOfRangeException(
        string operation)
    {
        var moment = MomentTestFactory.CreateMoment();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokeChange(
                moment,
                operation,
                MomentTestFactory.BaseTime.AddTicks(-1)));
    }

    private static Moment CreatePublishedMoment() =>
        MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                MomentTestFactory.BaseTime,
                MomentTestFactory.BaseTime));

    private static void InvokeChange(
        Moment moment,
        string operation,
        DateTimeOffset changedAt)
    {
        switch (operation)
        {
            case nameof(Moment.UpdateContent):
                moment.UpdateContent(
                    MomentTestFactory.CreateBody(),
                    [],
                    null,
                    changedAt);
                break;
            case nameof(Moment.ChangeVisibility):
                moment.ChangeVisibility(ContentVisibility.Private, changedAt);
                break;
            case nameof(Moment.ChangeAudienceRestriction):
                moment.ChangeAudienceRestriction(
                    AudienceRestrictionMode.Whitelist,
                    [],
                    changedAt);
                break;
            case nameof(Moment.ChangeDiscussion):
                moment.ChangeDiscussion(false, CommenterPolicy.AuthorOnly, changedAt);
                break;
            case nameof(Moment.ChangeTags):
                moment.ChangeTags([MomentTagIdentity.New()], changedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Moment change operation is not supported.");
        }
    }
}

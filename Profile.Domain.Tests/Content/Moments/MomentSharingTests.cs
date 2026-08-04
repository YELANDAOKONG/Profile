using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Moments;

public sealed class MomentSharingTests
{
    [Fact]
    public void CreateQuote_WithPublicTarget_CreatesDraftReferencingTarget()
    {
        var target = CreateShareableMoment();

        var quote = CreateQuote(target, UserIdentity.New());

        Assert.Equal(target.Id, quote.QuotedMomentId);
        Assert.Equal(PublicationStatus.Draft, quote.Publication.Status);
        Assert.NotNull(quote.Body);
    }

    [Fact]
    public void CreateQuote_WithOwnPublicMoment_AllowsValue()
    {
        var authorId = UserIdentity.New();
        var target = CreateShareableMoment(authorId);

        var quote = CreateQuote(target, authorId);

        Assert.Equal(authorId, quote.AuthorId);
        Assert.Equal(target.Id, quote.QuotedMomentId);
    }

    [Fact]
    public void CreateQuote_WithoutOwnBodyOrMedia_ThrowsArgumentException()
    {
        var target = CreateShareableMoment();

        Assert.Throws<ArgumentException>(
            () => Moment.CreateQuote(
                MomentIdentity.New(),
                UserIdentity.New(),
                null,
                [],
                ContentVisibility.Public,
                AudienceRestrictionMode.Blacklist,
                [],
                null,
                target,
                isBlockedBetweenAuthors: false,
                commentsAllowed: true,
                CommenterPolicy.AllReaders,
                [],
                MomentTestFactory.BaseTime));
    }

    [Fact]
    public void CreateQuote_WhileBlocked_ThrowsInvalidOperationException()
    {
        var target = CreateShareableMoment();

        Assert.Throws<InvalidOperationException>(
            () => CreateQuote(
                target,
                UserIdentity.New(),
                isBlockedBetweenAuthors: true));
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Deleted")]
    [InlineData("Followers")]
    [InlineData("Blacklist")]
    [InlineData("Whitelist")]
    public void CreateQuote_WithRestrictedOrUnavailableTarget_ThrowsArgumentException(
        string scenario)
    {
        var target = CreateNonShareableMoment(scenario);

        Assert.Throws<ArgumentException>(
            () => CreateQuote(target, UserIdentity.New()));
    }

    [Fact]
    public void MomentRepost_Create_WithPublicTarget_PreservesRelationship()
    {
        var target = CreateShareableMoment();
        var reposterId = UserIdentity.New();
        var repostedAt = MomentTestFactory.BaseTime.AddMinutes(1);

        var repost = MomentRepost.Create(
            reposterId,
            target,
            isBlockedBetweenAuthors: false,
            repostedAt);

        Assert.NotEqual(Guid.Empty, repost.Id.Value);
        Assert.Equal(reposterId, repost.ReposterId);
        Assert.Equal(target.Id, repost.MomentId);
        Assert.Equal(repostedAt, repost.RepostedAt);
    }

    [Fact]
    public void MomentRepost_CreateOwnMoment_AllowsValue()
    {
        var authorId = UserIdentity.New();
        var target = CreateShareableMoment(authorId);

        var repost = MomentRepost.Create(
            authorId,
            target,
            isBlockedBetweenAuthors: false,
            MomentTestFactory.BaseTime.AddMinutes(1));

        Assert.Equal(authorId, repost.ReposterId);
    }

    [Fact]
    public void MomentRepost_CreateSameTargetMultipleTimes_CreatesDistinctRelationships()
    {
        var target = CreateShareableMoment();
        var reposterId = UserIdentity.New();

        var first = MomentRepost.Create(
            reposterId,
            target,
            false,
            MomentTestFactory.BaseTime.AddMinutes(1));
        var second = MomentRepost.Create(
            reposterId,
            target,
            false,
            MomentTestFactory.BaseTime.AddMinutes(2));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(first.MomentId, second.MomentId);
        Assert.Equal(first.ReposterId, second.ReposterId);
    }

    [Fact]
    public void MomentRepost_CreateWhileBlocked_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => MomentRepost.Create(
                UserIdentity.New(),
                CreateShareableMoment(),
                true,
                MomentTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void MomentRepost_CreateWithRestrictedTarget_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => MomentRepost.Create(
                UserIdentity.New(),
                CreateNonShareableMoment("Blacklist"),
                false,
                MomentTestFactory.BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void IsPubliclyShareable_AfterVisibilityReduction_ReturnsFalse()
    {
        var target = CreateShareableMoment();

        target.ChangeVisibility(
            ContentVisibility.Followers,
            MomentTestFactory.BaseTime.AddMinutes(1));

        Assert.False(target.IsPubliclyShareable);
    }

    [Fact]
    public void IsPubliclyShareable_AfterDeleteAndRestore_RecoversValue()
    {
        var target = CreateShareableMoment();
        var deletedAt = MomentTestFactory.BaseTime.AddMinutes(1);
        target.Delete(deletedAt);

        Assert.False(target.IsPubliclyShareable);

        target.Restore(deletedAt.AddMinutes(1));

        Assert.True(target.IsPubliclyShareable);
    }

    private static Moment CreateQuote(
        Moment target,
        UserIdentity authorId,
        bool isBlockedBetweenAuthors = false) =>
        Moment.CreateQuote(
            MomentIdentity.New(),
            authorId,
            MomentTestFactory.CreateBody("Quote commentary"),
            [],
            ContentVisibility.Public,
            AudienceRestrictionMode.Blacklist,
            [],
            null,
            target,
            isBlockedBetweenAuthors,
            commentsAllowed: true,
            CommenterPolicy.AllReaders,
            [],
            MomentTestFactory.BaseTime);

    private static Moment CreateShareableMoment(UserIdentity? authorId = null) =>
        MomentTestFactory.ReconstituteMoment(
            Publication.Reconstitute(
                PublicationStatus.Published,
                null,
                MomentTestFactory.BaseTime,
                MomentTestFactory.BaseTime),
            authorId: authorId);

    private static Moment CreateNonShareableMoment(string scenario)
    {
        var published = Publication.Reconstitute(
            PublicationStatus.Published,
            null,
            MomentTestFactory.BaseTime,
            MomentTestFactory.BaseTime);

        return scenario switch
        {
            "Draft" => MomentTestFactory.CreateMoment(),
            "Deleted" => MomentTestFactory.ReconstituteMoment(
                published,
                ContentDeletion.Create(MomentTestFactory.BaseTime),
                updatedAt: MomentTestFactory.BaseTime),
            "Followers" => MomentTestFactory.ReconstituteMoment(
                published,
                visibility: ContentVisibility.Followers),
            "Blacklist" => MomentTestFactory.ReconstituteMoment(
                published,
                audienceAccountIds: [UserIdentity.New()]),
            "Whitelist" => MomentTestFactory.ReconstituteMoment(
                published,
                audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
                audienceAccountIds: [UserIdentity.New()]),
            _ => throw new ArgumentOutOfRangeException(
                nameof(scenario),
                scenario,
                "Moment sharing scenario is not supported.")
        };
    }
}

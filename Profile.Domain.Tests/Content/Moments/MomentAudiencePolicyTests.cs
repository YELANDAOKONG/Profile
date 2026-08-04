using Profile.Domain.Content.Moments;
using Profile.Domain.Content.Moments.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Moments;

public sealed class MomentAudiencePolicyTests
{
    [Fact]
    public void Blacklist_WithEmptySet_PreservesPublicAudience()
    {
        var moment = MomentTestFactory.ReconstituteMoment();

        Assert.True(
            MomentAudiencePolicy.IsMember(
                moment,
                null,
                false,
                false,
                false));
    }

    [Fact]
    public void Blacklist_WithListedViewer_SubtractsViewer()
    {
        var viewerId = UserIdentity.New();
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceAccountIds: [viewerId]);

        Assert.False(
            MomentAudiencePolicy.IsMember(
                moment,
                viewerId,
                false,
                false,
                false));
    }

    [Fact]
    public void Blacklist_WithUnlistedViewer_PreservesViewer()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceAccountIds: [UserIdentity.New()]);

        Assert.True(
            MomentAudiencePolicy.IsMember(
                moment,
                UserIdentity.New(),
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_WithListedViewer_IntersectsPublicAudience()
    {
        var viewerId = UserIdentity.New();
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.True(
            MomentAudiencePolicy.IsMember(
                moment,
                viewerId,
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_WithUnlistedViewer_ReturnsFalse()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [UserIdentity.New()]);

        Assert.False(
            MomentAudiencePolicy.IsMember(
                moment,
                UserIdentity.New(),
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_WithAnonymousViewer_ReturnsFalse()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [UserIdentity.New()]);

        Assert.False(
            MomentAudiencePolicy.IsMember(
                moment,
                null,
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_DoesNotExpandFollowersVisibility()
    {
        var viewerId = UserIdentity.New();
        var moment = MomentTestFactory.ReconstituteMoment(
            visibility: ContentVisibility.Followers,
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.False(
            MomentAudiencePolicy.IsMember(
                moment,
                viewerId,
                viewerFollowsAuthor: false,
                authorFollowsViewer: false,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void Whitelist_WithFollowerViewer_ReturnsTrue()
    {
        var viewerId = UserIdentity.New();
        var moment = MomentTestFactory.ReconstituteMoment(
            visibility: ContentVisibility.Followers,
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.True(
            MomentAudiencePolicy.IsMember(
                moment,
                viewerId,
                viewerFollowsAuthor: true,
                authorFollowsViewer: false,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void EmptyWhitelist_ProducesEmptyNonAuthorAudience()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist);

        Assert.False(
            MomentAudiencePolicy.IsMember(
                moment,
                UserIdentity.New(),
                false,
                false,
                false));
    }

    [Fact]
    public void Author_RemainsInAudienceWithEmptyWhitelist()
    {
        var moment = MomentTestFactory.ReconstituteMoment(
            visibility: ContentVisibility.Private,
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist);

        Assert.True(
            MomentAudiencePolicy.IsMember(
                moment,
                moment.AuthorId,
                false,
                false,
                false));
    }

    [Fact]
    public void PublicWhitelist_WithBlock_DoesNotHideListedViewer()
    {
        var viewerId = UserIdentity.New();
        var moment = MomentTestFactory.ReconstituteMoment(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.True(
            MomentAudiencePolicy.IsMember(
                moment,
                viewerId,
                false,
                false,
                true));
    }

    [Fact]
    public void FollowersAudience_WithBlock_ReturnsFalse()
    {
        var viewerId = UserIdentity.New();
        var moment = MomentTestFactory.ReconstituteMoment(
            visibility: ContentVisibility.Followers);

        Assert.False(
            MomentAudiencePolicy.IsMember(
                moment,
                viewerId,
                viewerFollowsAuthor: true,
                authorFollowsViewer: false,
                isBlockedBetweenAccounts: true));
    }
}

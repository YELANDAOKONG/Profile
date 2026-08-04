using Profile.Domain.Content.Posts;
using Profile.Domain.Content.Posts.Value;
using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Content.Posts;

public sealed class PostAudiencePolicyTests
{
    [Fact]
    public void Blacklist_WithEmptySet_PreservesPublicAudience()
    {
        var post = PostTestFactory.ReconstitutePost();

        Assert.True(
            PostAudiencePolicy.IsMember(
                post,
                null,
                false,
                false,
                false));
    }

    [Fact]
    public void Blacklist_WithListedViewer_SubtractsViewer()
    {
        var viewerId = UserIdentity.New();
        var post = PostTestFactory.ReconstitutePost(
            audienceAccountIds: [viewerId]);

        Assert.False(
            PostAudiencePolicy.IsMember(
                post,
                viewerId,
                false,
                false,
                false));
    }

    [Fact]
    public void Blacklist_WithUnlistedViewer_PreservesViewer()
    {
        var post = PostTestFactory.ReconstitutePost(
            audienceAccountIds: [UserIdentity.New()]);

        Assert.True(
            PostAudiencePolicy.IsMember(
                post,
                UserIdentity.New(),
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_WithListedViewer_IntersectsPublicAudience()
    {
        var viewerId = UserIdentity.New();
        var post = PostTestFactory.ReconstitutePost(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.True(
            PostAudiencePolicy.IsMember(
                post,
                viewerId,
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_WithUnlistedViewer_ReturnsFalse()
    {
        var post = PostTestFactory.ReconstitutePost(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [UserIdentity.New()]);

        Assert.False(
            PostAudiencePolicy.IsMember(
                post,
                UserIdentity.New(),
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_WithAnonymousViewer_ReturnsFalse()
    {
        var post = PostTestFactory.ReconstitutePost(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [UserIdentity.New()]);

        Assert.False(
            PostAudiencePolicy.IsMember(
                post,
                null,
                false,
                false,
                false));
    }

    [Fact]
    public void Whitelist_DoesNotExpandFollowersVisibility()
    {
        var viewerId = UserIdentity.New();
        var post = PostTestFactory.ReconstitutePost(
            visibility: ContentVisibility.Followers,
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.False(
            PostAudiencePolicy.IsMember(
                post,
                viewerId,
                viewerFollowsAuthor: false,
                authorFollowsViewer: false,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void Whitelist_WithFollowerViewer_ReturnsTrue()
    {
        var viewerId = UserIdentity.New();
        var post = PostTestFactory.ReconstitutePost(
            visibility: ContentVisibility.Followers,
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.True(
            PostAudiencePolicy.IsMember(
                post,
                viewerId,
                viewerFollowsAuthor: true,
                authorFollowsViewer: false,
                isBlockedBetweenAccounts: false));
    }

    [Fact]
    public void EmptyWhitelist_ProducesEmptyNonAuthorAudience()
    {
        var post = PostTestFactory.ReconstitutePost(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist);

        Assert.False(
            PostAudiencePolicy.IsMember(
                post,
                UserIdentity.New(),
                false,
                false,
                false));
    }

    [Fact]
    public void Author_RemainsInAudienceWithEmptyWhitelist()
    {
        var post = PostTestFactory.ReconstitutePost(
            visibility: ContentVisibility.Private,
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist);

        Assert.True(
            PostAudiencePolicy.IsMember(
                post,
                post.AuthorId,
                false,
                false,
                false));
    }

    [Fact]
    public void PublicWhitelist_WithBlock_DoesNotHideListedViewer()
    {
        var viewerId = UserIdentity.New();
        var post = PostTestFactory.ReconstitutePost(
            audienceRestrictionMode: AudienceRestrictionMode.Whitelist,
            audienceAccountIds: [viewerId]);

        Assert.True(
            PostAudiencePolicy.IsMember(
                post,
                viewerId,
                false,
                false,
                true));
    }

    [Fact]
    public void FollowersAudience_WithBlock_ReturnsFalse()
    {
        var viewerId = UserIdentity.New();
        var post = PostTestFactory.ReconstitutePost(
            visibility: ContentVisibility.Followers);

        Assert.False(
            PostAudiencePolicy.IsMember(
                post,
                viewerId,
                viewerFollowsAuthor: true,
                authorFollowsViewer: false,
                isBlockedBetweenAccounts: true));
    }
}

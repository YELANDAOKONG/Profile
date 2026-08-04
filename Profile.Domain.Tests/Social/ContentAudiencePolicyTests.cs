using Profile.Domain.Content.Value;
using Profile.Domain.Social;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Social;

public sealed class ContentAudiencePolicyTests
{
    [Fact]
    public void Public_WithAnonymousViewer_ReturnsTrue()
    {
        Assert.True(
            ContentAudiencePolicy.IsMember(
                ContentVisibility.Public,
                UserIdentity.New(),
                null,
                false,
                false,
                false));
    }

    [Fact]
    public void Public_WithBlock_ReturnsTrue()
    {
        Assert.True(
            ContentAudiencePolicy.IsMember(
                ContentVisibility.Public,
                UserIdentity.New(),
                UserIdentity.New(),
                false,
                false,
                true));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Followers_WithViewerFollowState_UsesCurrentRelationship(
        bool viewerFollowsAuthor,
        bool expected)
    {
        Assert.Equal(
            expected,
            ContentAudiencePolicy.IsMember(
                ContentVisibility.Followers,
                UserIdentity.New(),
                UserIdentity.New(),
                viewerFollowsAuthor,
                false,
                false));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void MutualFollowers_RequiresBothCurrentRelationships(
        bool viewerFollowsAuthor,
        bool authorFollowsViewer,
        bool expected)
    {
        Assert.Equal(
            expected,
            ContentAudiencePolicy.IsMember(
                ContentVisibility.MutualFollowers,
                UserIdentity.New(),
                UserIdentity.New(),
                viewerFollowsAuthor,
                authorFollowsViewer,
                false));
    }

    [Theory]
    [InlineData(ContentVisibility.Followers)]
    [InlineData(ContentVisibility.MutualFollowers)]
    public void RelationshipAudience_WithBlock_ReturnsFalse(
        ContentVisibility visibility)
    {
        Assert.False(
            ContentAudiencePolicy.IsMember(
                visibility,
                UserIdentity.New(),
                UserIdentity.New(),
                true,
                true,
                true));
    }

    [Theory]
    [InlineData(ContentVisibility.Followers)]
    [InlineData(ContentVisibility.MutualFollowers)]
    [InlineData(ContentVisibility.Private)]
    public void NonPublic_WithAnonymousViewer_ReturnsFalse(
        ContentVisibility visibility)
    {
        Assert.False(
            ContentAudiencePolicy.IsMember(
                visibility,
                UserIdentity.New(),
                null,
                true,
                true,
                false));
    }

    [Fact]
    public void Private_WithOtherViewer_ReturnsFalse()
    {
        Assert.False(
            ContentAudiencePolicy.IsMember(
                ContentVisibility.Private,
                UserIdentity.New(),
                UserIdentity.New(),
                true,
                true,
                false));
    }

    [Theory]
    [InlineData(ContentVisibility.Public)]
    [InlineData(ContentVisibility.Followers)]
    [InlineData(ContentVisibility.MutualFollowers)]
    [InlineData(ContentVisibility.Private)]
    public void AnyVisibility_WithAuthorViewer_ReturnsTrue(
        ContentVisibility visibility)
    {
        var authorId = UserIdentity.New();

        Assert.True(
            ContentAudiencePolicy.IsMember(
                visibility,
                authorId,
                authorId,
                false,
                false,
                false));
    }

    [Fact]
    public void UnsupportedVisibility_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ContentAudiencePolicy.IsMember(
                (ContentVisibility)999,
                UserIdentity.New(),
                UserIdentity.New(),
                false,
                false,
                false));
    }
}

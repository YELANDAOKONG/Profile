using Profile.Domain.Content.Value;

namespace Profile.Domain.Tests.Content.Value;

public sealed class ContentDeletionTests
{
    private static readonly DateTimeOffset _baseTime =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_FixesPurgeTimeAtFixedPeriodAfterDeletion()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.Equal(_baseTime, deletion.DeletedAt);
        Assert.Equal(
            _baseTime.AddDays(ContentDeletion.RecoveryPeriodDays),
            deletion.PurgeAt);
    }

    [Fact]
    public void CanRestoreAt_BeforeDeletion_ReturnsFalse()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.False(deletion.CanRestoreAt(_baseTime.AddTicks(-1)));
    }

    [Fact]
    public void CanRestoreAt_AtDeletion_ReturnsTrue()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.True(deletion.CanRestoreAt(_baseTime));
    }

    [Fact]
    public void CanRestoreAt_BeforePurge_ReturnsTrue()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.True(deletion.CanRestoreAt(deletion.PurgeAt.AddTicks(-1)));
    }

    [Fact]
    public void CanRestoreAt_AtPurge_ReturnsFalse()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.False(deletion.CanRestoreAt(deletion.PurgeAt));
    }

    [Fact]
    public void IsReadyForPurgeAt_BeforePurge_ReturnsFalse()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.False(deletion.IsReadyForPurgeAt(deletion.PurgeAt.AddTicks(-1)));
    }

    [Fact]
    public void IsReadyForPurgeAt_AtPurge_ReturnsTrue()
    {
        var deletion = ContentDeletion.Create(_baseTime);

        Assert.True(deletion.IsReadyForPurgeAt(deletion.PurgeAt));
    }

    [Fact]
    public void Reconstitute_WithMatchingTimes_PreservesValues()
    {
        var purgeAt = _baseTime.AddDays(ContentDeletion.RecoveryPeriodDays);

        var deletion = ContentDeletion.Reconstitute(_baseTime, purgeAt);

        Assert.Equal(_baseTime, deletion.DeletedAt);
        Assert.Equal(purgeAt, deletion.PurgeAt);
    }

    [Fact]
    public void Reconstitute_WithPurgeTimeOffFixedPeriod_ThrowsArgumentOutOfRangeException()
    {
        var purgeAt = _baseTime.AddDays(ContentDeletion.RecoveryPeriodDays + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ContentDeletion.Reconstitute(_baseTime, purgeAt));

        Assert.Equal("purgeAt", exception.ParamName);
    }

    [Fact]
    public void Reconstitute_WithPurgeTimeNotAfterDeletion_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ContentDeletion.Reconstitute(_baseTime, _baseTime));

        Assert.Equal("purgeAt", exception.ParamName);
    }
}

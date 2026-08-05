using Profile.Domain.Content.Collections;

namespace Profile.Domain.Tests.Content.Collections;

public sealed class SavedContentEligibilityPolicyTests
{
    [Fact]
    public void EnsureCanSave_WithEligibleTarget_AllowsOperation()
    {
        SavedContentEligibilityPolicy.EnsureCanSave(
            isTargetActiveAndPublished: true,
            isInReadingAudience: true,
            isBlockedBetweenAccounts: false);
    }

    [Theory]
    [InlineData(false, true, false, typeof(ArgumentException))]
    [InlineData(true, false, false, typeof(InvalidOperationException))]
    [InlineData(true, true, true, typeof(InvalidOperationException))]
    public void EnsureCanSave_WithIneligibleTarget_ThrowsExpectedException(
        bool isTargetActiveAndPublished,
        bool isInReadingAudience,
        bool isBlockedBetweenAccounts,
        Type exceptionType)
    {
        Assert.Throws(
            exceptionType,
            () => SavedContentEligibilityPolicy.EnsureCanSave(
                isTargetActiveAndPublished,
                isInReadingAudience,
                isBlockedBetweenAccounts));
    }
}

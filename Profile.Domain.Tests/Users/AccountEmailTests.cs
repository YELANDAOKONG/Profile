using Profile.Domain.Users;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Tests.Users;

public sealed class AccountEmailTests
{
    [Fact]
    public void Constructor_WithUnverifiedAddress_PreservesData()
    {
        var address = new EmailAddress("user@example.com");

        var email = new AccountEmail(address, null);

        Assert.Equal(address, email.Address);
        Assert.Null(email.VerifiedAt);
    }

    [Fact]
    public void Constructor_WithVerifiedAddress_PreservesData()
    {
        var address = new EmailAddress("user@example.com");
        var verifiedAt = DateTimeOffset.UtcNow;

        var email = new AccountEmail(address, verifiedAt);

        Assert.Equal(address, email.Address);
        Assert.Equal(verifiedAt, email.VerifiedAt);
    }

    [Fact]
    public void Equality_WithSameData_TreatsValuesAsEqual()
    {
        var address = new EmailAddress("user@example.com");
        var verifiedAt = DateTimeOffset.UtcNow;

        var first = new AccountEmail(address, verifiedAt);
        var second = new AccountEmail(address, verifiedAt);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Constructor_WithNullAddress_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AccountEmail(null!, null));
    }
}

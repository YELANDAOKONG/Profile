using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class Account
{
    public required UserIdentity Id { get; init; }

    public required StringIdentity StringId { get; init; }

    public required AccountEmail Email { get; init; }

    public required AccountRole Role { get; init; }

    
    // Account Status
    public AccountSuspension? Suspension { get; init; }

    public AccountBan? Ban { get; init; }

    public AccountDeletion? Deletion { get; init; }

    
    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

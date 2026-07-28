using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class Account
{
    public required UserIdentity Id { get; init; }
    
    public required StringIdentity StringId { get; init; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
}
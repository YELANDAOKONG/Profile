using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class Account
{
    public required UserIdentity Id { get; init; }
    
    public required StringIdentity StringId { get; init; }
    
    public required AccountEmail Email { get; init; }
    
    
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset UpdatedAt { get; private set; }
    
    public DateTimeOffset? DeletedAt { get; private set; }
}
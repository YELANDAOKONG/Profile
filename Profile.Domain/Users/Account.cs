using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class Account
{
    public UserIdentity Id { get; init; }
    
    public StringIdentity StringId { get; private set; }
    
    public AccountEmail Email { get; private set; }
    
    
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset UpdatedAt { get; private set; }
    
    public DateTimeOffset? DeletedAt { get; private set; }
}
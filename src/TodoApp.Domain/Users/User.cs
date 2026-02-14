namespace TodoApp.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    
    private readonly List<RefreshToken> _refreshTokens = new();
    public List<RefreshToken> RefreshTokens => _refreshTokens;
    
    // needed by EF Core (private parameterless ctor)
    private User () { }
    
    public User (string email, string passwordHash)
    {
        SetEmail(email);
        SetPasswordHash(passwordHash);
    }
    
    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email must not be empty.");

        Email = email.Trim();
    }
    
    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash must not be empty.");

        PasswordHash = passwordHash;
    }
}
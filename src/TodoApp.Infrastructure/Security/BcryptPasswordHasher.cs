using TodoApp.Application.Abstractions.Security;

namespace TodoApp.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private readonly BCrypt.Net.HashType _hashType = BCrypt.Net.HashType.SHA384; // optional: choose a variant

    public string Hash(string password)
    {
        // WorkFactor default is okay for learning; raise for production accordingly.
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
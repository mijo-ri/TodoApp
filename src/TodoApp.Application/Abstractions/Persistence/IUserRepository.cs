using TodoApp.Domain.Users;

namespace TodoApp.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    void Add(User user);
    void Remove(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
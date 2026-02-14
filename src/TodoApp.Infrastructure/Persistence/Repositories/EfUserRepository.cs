using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Domain.Users;

namespace TodoApp.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly TodoDbContext _db;
    
    public EfUserRepository(TodoDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public void Add(User user)
    {
        _db.Users.Add(user);
    }

    public void Remove(User user)
    {
        _db.Users.Remove(user);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
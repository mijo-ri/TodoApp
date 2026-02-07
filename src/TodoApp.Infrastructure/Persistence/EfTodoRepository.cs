using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Domain.Todos;

namespace TodoApp.Infrastructure.Persistence;

public sealed class EfTodoRepository : ITodoRepository
{
    private readonly TodoDbContext _db;

    public EfTodoRepository(TodoDbContext db)
    {
        _db = db;
    }

    public Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Todos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<TodoItem>> ListAsync(
        bool? isCompleted,
        CancellationToken cancellationToken)
    {
        var query = _db.Todos.AsQueryable();

        if (isCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == isCompleted.Value);

        return query
            .OrderBy(x => x.IsCompleted)
            .ThenBy(x => x.DueDate)
            .ToListAsync(cancellationToken);
    }

    public void Add(TodoItem item)
    {
        _db.Todos.Add(item);
    }

    public void Remove(TodoItem item)
    {
        _db.Todos.Remove(item);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}

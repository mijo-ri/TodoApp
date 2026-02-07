using TodoApp.Domain.Todos;

namespace TodoApp.Application.Abstractions.Persistence;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TodoItem>> ListAsync(bool? isCompleted, CancellationToken cancellationToken);

    void Add(TodoItem item);
    void Remove(TodoItem item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

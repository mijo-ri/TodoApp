using TodoApp.Domain.Todos;

namespace TodoApp.Application.Abstractions.Persistence;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TodoItem?> GetByIdForOwnerAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken);
    
    Task<List<TodoItem>> ListAsync(
        Guid ownerId,
        bool? isCompleted,
        CancellationToken cancellationToken);

    void Add(TodoItem item);
    void Remove(TodoItem item);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

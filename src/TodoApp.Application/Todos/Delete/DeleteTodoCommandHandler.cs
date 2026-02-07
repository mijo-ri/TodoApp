using ErrorOr;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.Delete;

public sealed class DeleteTodoCommandHandler
    : IRequestHandler<DeleteTodoCommand, ErrorOr<Deleted>>
{
    private readonly ITodoRepository _todoRepository;

    public DeleteTodoCommandHandler(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        DeleteTodoCommand request,
        CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (todo is null)
            return TodoErrors.NotFound(request.Id);

        _todoRepository.Remove(todo);        
        await _todoRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}

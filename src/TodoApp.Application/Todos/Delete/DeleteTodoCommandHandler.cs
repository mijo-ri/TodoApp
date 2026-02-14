using ErrorOr;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.Delete;

public sealed class DeleteTodoCommandHandler
    : IRequestHandler<DeleteTodoCommand, ErrorOr<Deleted>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTodoCommandHandler(
        ITodoRepository todoRepository,
        ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        DeleteTodoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return UserErrors.Unauthorized();

        var todo = await _todoRepository.GetByIdForOwnerAsync(
            request.Id,
            userId.Value,
            cancellationToken);

        if (todo is null)
            return TodoErrors.NotFound(request.Id);

        _todoRepository.Remove(todo);        
        await _todoRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}

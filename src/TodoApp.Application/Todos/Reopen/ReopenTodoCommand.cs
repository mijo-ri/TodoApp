using ErrorOr;
using MediatR;

namespace TodoApp.Application.Todos.Reopen;

public sealed record ReopenTodoCommand(
    Guid Id
) : IRequest<ErrorOr<TodoDto>>;
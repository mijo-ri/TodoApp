using ErrorOr;
using MediatR;

namespace TodoApp.Application.Todos.Complete;

public sealed record CompleteTodoCommand(
    Guid Id
) : IRequest<ErrorOr<TodoDto>>;
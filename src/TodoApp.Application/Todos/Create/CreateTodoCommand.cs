using ErrorOr;
using MediatR;

namespace TodoApp.Application.Todos.Create;

public sealed record CreateTodoCommand(
    string Title,
    string? Notes,
    DateOnly? DueDate
) : IRequest<ErrorOr<TodoDto>>;

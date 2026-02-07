using ErrorOr;
using MediatR;

namespace TodoApp.Application.Todos.Update;

public sealed record UpdateTodoCommand(
    Guid Id,
    string Title,
    string? Notes,
    DateOnly? DueDate
) : IRequest<ErrorOr<TodoDto>>;

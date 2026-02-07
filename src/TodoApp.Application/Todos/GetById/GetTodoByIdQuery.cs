using ErrorOr;
using MediatR;

namespace TodoApp.Application.Todos.GetById;

public sealed record GetTodoByIdQuery(Guid Id)
    : IRequest<ErrorOr<TodoDto>>;

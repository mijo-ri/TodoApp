using ErrorOr;
using MediatR;

namespace TodoApp.Application.Todos.Delete;

public sealed record DeleteTodoCommand(Guid Id)
    : IRequest<ErrorOr<Deleted>>;

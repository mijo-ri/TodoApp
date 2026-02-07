using MediatR;

namespace TodoApp.Application.Todos.List;

public sealed record ListTodosQuery(bool? IsCompleted)
    : IRequest<List<TodoDto>>;

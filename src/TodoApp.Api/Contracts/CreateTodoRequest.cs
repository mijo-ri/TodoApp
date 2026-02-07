namespace TodoApp.Api.Contracts;

public sealed record CreateTodoRequest(
    string Title,
    string? Notes,
    DateOnly? DueDate
);
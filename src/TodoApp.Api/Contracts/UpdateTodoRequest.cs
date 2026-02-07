namespace TodoApp.Api.Contracts;

public sealed record UpdateTodoRequest(
    string Title,
    string? Notes,
    DateOnly? DueDate
);
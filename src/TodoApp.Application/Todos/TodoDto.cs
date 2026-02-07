namespace TodoApp.Application.Todos;

public sealed record TodoDto(
    Guid Id,
    string Title,
    string? Notes,
    DateOnly? DueDate,
    bool IsCompleted,
    DateTimeOffset? CompletedAt
);

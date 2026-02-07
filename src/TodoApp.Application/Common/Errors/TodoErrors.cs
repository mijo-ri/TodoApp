using ErrorOr;

namespace TodoApp.Application.Common.Errors;

public static class TodoErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound(
            code: "Todo.NotFound",
            description: $"Todo '{id}' not found.");

    public static Error Domain(string message) =>
        Error.Validation(
            code: "Todo.DomainValidation",
            description: message);
}

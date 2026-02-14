using ErrorOr;

namespace TodoApp.Application.Common.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound(
            code: "User.NotFound",
            description: $"User '{id}' not found.");

    public static Error Domain(string message) =>
        Error.Validation(
            code: "User.DomainValidation",
            description: message);
    
    public static Error DuplicateEmail(string email) =>
        Error.Conflict(
            code: "User.DuplicateEmail",
            description: $"Email '{email}' is already in use.");
    
    public static Error InvalidCredentials() =>
        Error.Unauthorized(
            code: "User.InvalidCredentials",
            description: "Invalid email or password.");
    
    public static Error InvalidRefreshToken() =>
        Error.Unauthorized(
            code: "User.InvalidRefreshToken",
            description: "Invalid refresh token.");
    
    public static Error Unauthorized() =>
        Error.Unauthorized(
            code: "User.Unauthorized",
            description: "User is not authenticated.");
}
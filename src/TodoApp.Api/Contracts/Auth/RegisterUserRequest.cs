namespace TodoApp.Api.Contracts.Auth;

public sealed record RegisterUserRequest(string Email, string Password);
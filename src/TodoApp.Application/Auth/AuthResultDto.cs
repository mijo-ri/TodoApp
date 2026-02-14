namespace TodoApp.Application.Auth;

public record AuthResultDto(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
namespace TodoApp.Application.Abstractions.Security;

public record TokenResult(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);

public record UserRefreshInfo(Guid RefreshTokenId, Guid UserId);

public interface ITokenService
{
    Task<TokenResult> CreateTokenAsync(Guid userId, string ipAddress);
    Task<(bool ok, UserRefreshInfo? info)> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null);
}
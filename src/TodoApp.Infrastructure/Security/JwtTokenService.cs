using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Domain.Users;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly TodoDbContext _db;
    private readonly byte[] _key;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public JwtTokenService(IConfiguration config, TodoDbContext db)
    {
        _config = config;
        _db = db;

        var secret = _config["Jwt:Secret"] ?? 
            throw new ArgumentNullException("Jwt:Secret not configured");
        _key = Encoding.UTF8.GetBytes(secret);
        _issuer = _config["Jwt:Issuer"];
        _audience = _config["Jwt:Audience"];
        _accessTokenMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m)
            ? m 
            : 15;
        _refreshTokenDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d)
            ? d
            : 30;
    }

    public async Task<TokenResult> CreateTokenAsync(Guid userId, string ipAddress)
    {
        var accessTokenExpires = DateTimeOffset.UtcNow.AddMinutes(_accessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: accessTokenExpires.UtcDateTime,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenString = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = refreshTokenString,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_refreshTokenDays),
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync();

        return new TokenResult(accessToken, refreshTokenString, accessTokenExpires);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<(bool ok, UserRefreshInfo? info)> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Where(t => t.Token == refreshToken)
            .Select(t => new
            {
                t.Id,
                t.IsRevoked,
                t.ExpiresAt,
                UserId = EF.Property<Guid>(t, "UserId")
            })
            .FirstOrDefaultAsync();

        if (token is null) return (false, null);
        if (token.IsRevoked) return (false, null);
        if (token.ExpiresAt < DateTimeOffset.UtcNow) return (false, null);

        var info = new UserRefreshInfo(token.Id, token.UserId);
        return (true, info);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null)
    {
        var entity = await _db.RefreshTokens.FirstOrDefaultAsync(
            t => t.Token == refreshToken);
        if (entity is null) return;
        entity.IsRevoked = true;
        entity.ReplacedByToken = replacedByToken;
        await _db.SaveChangesAsync();
    }
}
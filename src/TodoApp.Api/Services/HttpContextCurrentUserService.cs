using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TodoApp.Application.Abstractions.Security;

namespace TodoApp.Api.Services;

/// <summary>
/// Reads the current user id from HttpContext.User claims.
/// Looks for JwtRegisteredClaimNames.Sub or ClaimTypes.NameIdentifier.
/// </summary>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null) return null;

            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated != true) return null;

            // Try sub (JWT) first, then NameIdentifier
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrWhiteSpace(sub)) return null;

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
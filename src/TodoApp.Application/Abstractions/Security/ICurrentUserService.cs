using System;

namespace TodoApp.Application.Abstractions.Security;

/// <summary>
/// Abstraction for accessing current user information (from claims).
/// Implemented in the API project (HttpContext-based).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The current authenticated user's Id if available, otherwise null.
    /// </summary>
    Guid? UserId { get; }
}
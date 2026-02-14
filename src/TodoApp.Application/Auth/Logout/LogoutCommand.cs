using ErrorOr;
using MediatR;

namespace TodoApp.Application.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<ErrorOr<bool>>;
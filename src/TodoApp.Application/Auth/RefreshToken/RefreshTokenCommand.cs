using ErrorOr;
using MediatR;

namespace TodoApp.Application.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, string IpAddress)
    : IRequest<ErrorOr<AuthResultDto>>;
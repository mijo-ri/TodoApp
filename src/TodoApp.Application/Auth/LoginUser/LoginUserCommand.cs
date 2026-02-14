using ErrorOr;
using MediatR;

namespace TodoApp.Application.Auth.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password,
    string IpAddress
) : IRequest<ErrorOr<AuthResultDto>>;
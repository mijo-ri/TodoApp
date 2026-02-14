using ErrorOr;
using MediatR;
using TodoApp.Application.Abstractions.Security;

namespace TodoApp.Application.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, ErrorOr<bool>>
{
    private readonly ITokenService _tokenService;
    
    public LogoutCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    public async Task<ErrorOr<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        return true;
    }
}
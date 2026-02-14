using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthResultDto>>
{
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(ITokenService tokenService, IMapper mapper)
    {
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<AuthResultDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var (ok, info) = await _tokenService.ValidateRefreshTokenAsync(
            request.RefreshToken);

        if (!ok || info is null)
        {
            return UserErrors.InvalidRefreshToken();
        }

        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        var newTokens = await _tokenService.CreateTokenAsync(
            info.UserId,
            request.IpAddress);
        
        return _mapper.Map<AuthResultDto>(newTokens);
    }
}
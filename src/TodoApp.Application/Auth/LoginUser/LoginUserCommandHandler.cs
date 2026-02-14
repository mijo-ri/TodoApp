using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Auth.LoginUser;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ErrorOr<AuthResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<ErrorOr<AuthResultDto>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return UserErrors.InvalidCredentials();
        }
        
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return UserErrors.InvalidCredentials();
        }

        var tokens = await _tokenService.CreateTokenAsync(user.Id, request.IpAddress);

        return _mapper.Map<AuthResultDto>(tokens);
    }
}
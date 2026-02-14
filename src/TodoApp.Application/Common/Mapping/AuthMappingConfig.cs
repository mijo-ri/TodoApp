using Mapster;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Auth;
using TodoApp.Domain.Users;

namespace TodoApp.Application.Common.Mapping;

public sealed class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TokenResult, AuthResultDto>()
            .Map(dest => dest.AccessToken, src => src.AccessToken)
            .Map(dest => dest.RefreshToken, src => src.RefreshToken)
            .Map(dest => dest.ExpiresAt, src => src.AccessTokenExpiresAt);
    }
}

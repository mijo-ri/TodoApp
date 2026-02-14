using Mapster;
using TodoApp.Application.Auth;
using TodoApp.Domain.Users;

namespace TodoApp.Application.Common.Mapping;

public sealed class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email);
    }
}

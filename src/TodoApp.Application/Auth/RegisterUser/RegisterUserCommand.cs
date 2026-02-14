using ErrorOr;
using MediatR;

namespace TodoApp.Application.Auth.RegisterUser;

public record RegisterUserCommand(string Email, string Password) : IRequest<ErrorOr<UserDto>>;
using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Contracts.Auth;
using TodoApp.Api.Errors;
using TodoApp.Application.Auth;
using TodoApp.Application.Auth.LoginUser;
using TodoApp.Application.Auth.Logout;
using TodoApp.Application.Auth.RefreshToken;
using TodoApp.Application.Auth.RegisterUser;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed partial class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            user => CreatedAtAction(null, new { id = user.Id }),
            errors => this.ProblemFromErrors(errors));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new LoginUserCommand(request.Email, request.Password, ipAddress);
        var result = await _mediator.Send(command);

        return result.ToActionResult(this);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);
        var result = await _mediator.Send(command);

        return result.ToActionResult(this);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await _mediator.Send(command);

        return result.Match<IActionResult>(
            ok => NoContent(),
            errors => this.ProblemFromErrors(errors));
    }
}
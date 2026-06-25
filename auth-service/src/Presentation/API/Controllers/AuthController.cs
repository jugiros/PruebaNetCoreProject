using AuthService.Application.UseCases.Commands.Login;
using AuthService.Application.UseCases.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
            return UnprocessableEntity(new { error = result.Error });

        return CreatedAtAction(
            nameof(Register),
            new { id = result.Value },
            new RegisterUserResponse(result.Value!));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new LoginCommand(request.Email, request.Password);
        var result  = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(new LoginResponse(
            result.Value!.AccessToken,
            result.Value.RefreshToken,
            result.Value.UserId,
            result.Value.FullName));
    }
}

public record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);
public record RegisterUserResponse(Guid UserId);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, Guid UserId, string FullName);

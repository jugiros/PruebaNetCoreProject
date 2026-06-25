using AuthService.Application.Common;
using MediatR;

namespace AuthService.Application.UseCases.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResult>>;

public record LoginResult(string AccessToken, string RefreshToken, Guid UserId, string FullName);

using AuthService.Application.Common;
using MediatR;

namespace AuthService.Application.UseCases.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<Guid>>;

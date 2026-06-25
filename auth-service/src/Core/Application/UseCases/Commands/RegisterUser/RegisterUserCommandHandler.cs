using AuthService.Application.Common;
using AuthService.Application.Ports;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.UseCases.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<RegisterUserCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(logger);
        _userRepository  = userRepository;
        _passwordHasher  = passwordHasher;
        _logger          = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Email.Create(request.Email);

        var exists = await _userRepository.ExistsByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (exists)
            return Result<Guid>.Failure("Ya existe un usuario registrado con ese correo electrónico.");

        var fullName     = FullName.Create(request.FirstName, request.LastName);
        var hash         = _passwordHasher.Hash(request.Password);
        var passwordHash = PasswordHash.FromHash(hash);
        var user         = User.Create(email, fullName, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Usuario registrado — Id: {UserId}", user.Id);

        return Result<Guid>.Success(user.Id);
    }
}

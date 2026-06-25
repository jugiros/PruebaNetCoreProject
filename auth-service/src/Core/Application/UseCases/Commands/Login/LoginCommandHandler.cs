using AuthService.Application.Common;
using AuthService.Application.Ports;
using AuthService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.UseCases.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService   _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(tokenService);
        ArgumentNullException.ThrowIfNull(logger);
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService   = tokenService;
        _logger         = logger;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Email.Create(request.Email);
        var user  = await _userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash.Value))
        {
            _logger.LogWarning("Intento de login fallido para: {Email}", request.Email);
            return Result<LoginResult>.Failure("Credenciales inválidas.");
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);

        var accessToken  = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _logger.LogInformation("Login exitoso — UserId: {UserId}", user.Id);

        return Result<LoginResult>.Success(new LoginResult(
            accessToken,
            refreshToken,
            user.Id,
            user.FullName.Value));
    }
}

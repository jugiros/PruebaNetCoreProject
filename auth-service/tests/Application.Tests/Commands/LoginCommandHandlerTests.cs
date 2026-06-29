using AuthService.Application.Ports;
using AuthService.Application.UseCases.Commands.Login;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AuthService.Application.Tests.Commands;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _repository  = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher      = Substitute.For<IPasswordHasher>();
    private readonly ITokenService   _tokenService = Substitute.For<ITokenService>();
    private readonly ILogger<LoginCommandHandler> _logger = Substitute.For<ILogger<LoginCommandHandler>>();

    private LoginCommandHandler BuildHandler() =>
        new(_repository, _hasher, _tokenService, _logger);

    private static User BuildActiveUser() => User.Create(
        Email.Create("user@example.com"),
        FullName.Create("Juan", "Pérez"),
        PasswordHash.FromHash("$2a$12$hashedvalue"));

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithTokens()
    {
        var user = BuildActiveUser();
        _repository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
                   .Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _tokenService.GenerateAccessToken(user).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");

        var result = await BuildHandler().Handle(
            new LoginCommand("user@example.com", "Password1!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        _repository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
                   .Returns((User?)null);

        var result = await BuildHandler().Handle(
            new LoginCommand("notfound@example.com", "Password1!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        var user = BuildActiveUser();
        _repository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
                   .Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await BuildHandler().Handle(
            new LoginCommand("user@example.com", "WrongPass!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Credenciales inválidas");
        _tokenService.DidNotReceive().GenerateAccessToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_NullRequest_ThrowsArgumentNullException()
    {
        var act = async () => await BuildHandler().Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

using AuthService.Application.Ports;
using AuthService.Application.UseCases.Commands.RegisterUser;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AuthService.Application.Tests.Commands;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository   _repository    = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher   _hasher        = Substitute.For<IPasswordHasher>();
    private readonly ILogger<RegisterUserCommandHandler> _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();

    private RegisterUserCommandHandler BuildHandler() =>
        new(_repository, _hasher, _logger);

    private static RegisterUserCommand ValidCommand() =>
        new("newuser@example.com", "Password1!", "Juan", "Pérez");

    [Fact]
    public async Task Handle_NewUser_ReturnsSuccessWithUserId()
    {
        _repository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
                   .Returns(false);
        _hasher.Hash(Arg.Any<string>()).Returns("$2a$12$hashedvalue");

        var result = await BuildHandler().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        _repository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
                   .Returns(true);

        var result = await BuildHandler().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Ya existe");
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NullRequest_ThrowsArgumentNullException()
    {
        var act = async () => await BuildHandler().Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

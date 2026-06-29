using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions;
using AuthService.Domain.ValueObjects;
using FluentAssertions;

namespace AuthService.Domain.Tests.Entities;

public sealed class UserTests
{
    private static User BuildUser() => User.Create(
        Email.Create("test@example.com"),
        FullName.Create("Juan", "Pérez"),
        PasswordHash.FromHash("$2a$12$hashedvalue"));

    [Fact]
    public void Create_ValidInputs_ReturnsActiveUser()
    {
        var user = BuildUser();

        user.Id.Should().NotBeEmpty();
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Create_NullEmail_ThrowsArgumentNullException()
    {
        var act = () => User.Create(null!, FullName.Create("Juan", "Pérez"), PasswordHash.FromHash("hash"));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordLogin_ActiveUser_SetsLastLoginAt()
    {
        var user = BuildUser();

        user.RecordLogin();

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordLogin_InactiveUser_ThrowsDomainException()
    {
        var user = BuildUser();
        user.Deactivate();

        var act = () => user.RecordLogin();

        act.Should().Throw<DomainException>()
           .WithMessage("*desactivado*");
    }

    [Fact]
    public void Deactivate_ActiveUser_SetsIsActiveFalse()
    {
        var user = BuildUser();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var user = BuildUser();
        user.Deactivate();

        var act = () => user.Deactivate();

        act.Should().Throw<DomainException>()
           .WithMessage("*ya está desactivado*");
    }

    [Fact]
    public void UpdatePassword_NewHash_UpdatesPasswordHash()
    {
        var user    = BuildUser();
        var newHash = PasswordHash.FromHash("$2a$12$newhashvalue");

        user.UpdatePassword(newHash);

        user.PasswordHash.Should().Be(newHash);
        user.UpdatedAt.Should().NotBeNull();
    }
}

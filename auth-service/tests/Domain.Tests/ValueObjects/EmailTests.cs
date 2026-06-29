using AuthService.Domain.Exceptions;
using AuthService.Domain.ValueObjects;
using FluentAssertions;

namespace AuthService.Domain.Tests.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user.name+tag@sub.domain.io")]
    public void Create_ValidEmail_ReturnsNormalizedLowercase(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var email = Email.Create(input);

        email.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ThrowsDomainException(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<DomainException>()
           .WithMessage("*vacío*");
    }

    [Fact]
    public void Create_NullEmail_ThrowsDomainException()
    {
        var act = () => Email.Create(null!);

        act.Should().Throw<DomainException>()
           .WithMessage("*vacío*");
    }

    [Fact]
    public void Create_ExceedsMaxLength_ThrowsDomainException()
    {
        var longEmail = new string('a', 250) + "@b.com";

        var act = () => Email.Create(longEmail);

        act.Should().Throw<DomainException>()
           .WithMessage("*longitud máxima*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("missing@")]
    public void Create_InvalidFormat_ThrowsDomainException(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<DomainException>()
           .WithMessage("*formato*");
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = Email.Create("test@example.com");
        var b = Email.Create("TEST@EXAMPLE.COM");

        a.Should().Be(b);
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = Email.Create("a@example.com");
        var b = Email.Create("b@example.com");

        a.Should().NotBe(b);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var email = Email.Create("user@example.com");

        email.ToString().Should().Be("user@example.com");
    }
}

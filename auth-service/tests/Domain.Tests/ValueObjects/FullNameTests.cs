using AuthService.Domain.Exceptions;
using AuthService.Domain.ValueObjects;
using FluentAssertions;

namespace AuthService.Domain.Tests.ValueObjects;

public sealed class FullNameTests
{
    [Fact]
    public void Create_ValidNames_ReturnsTrimmedValue()
    {
        var fullName = FullName.Create("  Juan  ", "  Pérez  ");

        fullName.FirstName.Should().Be("Juan");
        fullName.LastName.Should().Be("Pérez");
        fullName.Value.Should().Be("Juan Pérez");
    }

    [Theory]
    [InlineData("", "Pérez")]
    [InlineData("   ", "Pérez")]
    public void Create_EmptyFirstName_ThrowsDomainException(string firstName, string lastName)
    {
        var act = () => FullName.Create(firstName, lastName);

        act.Should().Throw<DomainException>()
           .WithMessage("*nombre*vacío*");
    }

    [Theory]
    [InlineData("Juan", "")]
    [InlineData("Juan", "   ")]
    public void Create_EmptyLastName_ThrowsDomainException(string firstName, string lastName)
    {
        var act = () => FullName.Create(firstName, lastName);

        act.Should().Throw<DomainException>()
           .WithMessage("*apellido*vacío*");
    }

    [Fact]
    public void Create_FirstNameExceedsMaxLength_ThrowsDomainException()
    {
        var act = () => FullName.Create(new string('a', 101), "Pérez");

        act.Should().Throw<DomainException>()
           .WithMessage("*nombre*100*");
    }

    [Fact]
    public void Create_LastNameExceedsMaxLength_ThrowsDomainException()
    {
        var act = () => FullName.Create("Juan", new string('a', 101));

        act.Should().Throw<DomainException>()
           .WithMessage("*apellido*100*");
    }

    [Fact]
    public void Equals_SameFullName_ReturnsTrue()
    {
        var a = FullName.Create("Juan", "Pérez");
        var b = FullName.Create("Juan", "Pérez");

        a.Should().Be(b);
    }
}

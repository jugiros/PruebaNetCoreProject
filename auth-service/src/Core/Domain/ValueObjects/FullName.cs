using AuthService.Domain.Exceptions;

namespace AuthService.Domain.ValueObjects;

public sealed class FullName : IEquatable<FullName>
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Value => $"{FirstName} {LastName}";

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static FullName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido no puede estar vacío.");

        if (firstName.Length > 100)
            throw new DomainException("El nombre excede la longitud máxima de 100 caracteres.");

        if (lastName.Length > 100)
            throw new DomainException("El apellido excede la longitud máxima de 100 caracteres.");

        return new FullName(firstName.Trim(), lastName.Trim());
    }

    public bool Equals(FullName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is FullName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;
}

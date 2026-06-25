using AuthService.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace AuthService.Domain.ValueObjects;

public sealed partial class Email : IEquatable<Email>
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El correo electrónico no puede estar vacío.");

        if (value.Length > 254)
            throw new DomainException("El correo electrónico excede la longitud máxima permitida.");

        if (!EmailPattern().IsMatch(value))
            throw new DomainException("El formato del correo electrónico no es válido.");

        return new Email(value.ToLowerInvariant());
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;
}

using AuthService.Domain.Exceptions;

namespace AuthService.Domain.ValueObjects;

public sealed class PasswordHash : IEquatable<PasswordHash>
{
    public string Value { get; }

    private PasswordHash(string value) => Value = value;

    public static PasswordHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("El hash de contraseña no puede estar vacío.");

        return new PasswordHash(hash);
    }

    public bool Equals(PasswordHash? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PasswordHash other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => "[PROTECTED]";
}

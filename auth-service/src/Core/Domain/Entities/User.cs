using AuthService.Domain.Common;
using AuthService.Domain.Exceptions;
using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities;

public sealed class User : AggregateRoot
{
    public Email Email { get; private set; } = null!;
    public FullName FullName { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    public static User Create(Email email, FullName fullName, PasswordHash passwordHash)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(passwordHash);

        return new User
        {
            Id           = Guid.NewGuid(),
            Email        = email,
            FullName     = fullName,
            PasswordHash = passwordHash,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };
    }

    public void RecordLogin()
    {
        if (!IsActive)
            throw new DomainException("El usuario está desactivado y no puede iniciar sesión.");

        LastLoginAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("El usuario ya está desactivado.");

        IsActive = false;
        MarkUpdated();
    }

    public void UpdatePassword(PasswordHash newPasswordHash)
    {
        ArgumentNullException.ThrowIfNull(newPasswordHash);

        PasswordHash = newPasswordHash;
        MarkUpdated();
    }
}

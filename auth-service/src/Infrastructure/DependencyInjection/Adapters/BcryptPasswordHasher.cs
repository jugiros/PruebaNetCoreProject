using AuthService.Application.Ports;

namespace AuthService.Infrastructure.DependencyInjection.Adapters;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

    public bool Verify(string plainPassword, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, hash);
}

using AuthService.Application.Ports;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.DependencyInjection.Adapters;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var secret  = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret no configurado.");
        var issuer  = _configuration["Jwt:Issuer"]  ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
        var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado.");
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("scope", "auth:read"),
            new Claim("fullName", user.FullName.Value)
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public Guid? GetUserIdFromExpiredToken(string token)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret no configurado.");

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer          = false,
            ValidateAudience        = false,
            ValidateLifetime        = false,
            ClockSkew               = TimeSpan.Zero
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParams, out _);

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}

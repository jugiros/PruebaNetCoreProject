using AuthService.Domain.Entities;

namespace AuthService.Application.Ports;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? GetUserIdFromExpiredToken(string token);
}

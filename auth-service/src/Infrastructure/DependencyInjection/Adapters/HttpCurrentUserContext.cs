using AuthService.Application.Ports;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Infrastructure.DependencyInjection.Adapters;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

    public string Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

    public IReadOnlyCollection<string> Scopes =>
        _httpContextAccessor.HttpContext?.User.FindAll("scope")
            .Select(c => c.Value).ToList().AsReadOnly()
        ?? (IReadOnlyCollection<string>)Array.Empty<string>();

    public bool HasScope(string scope) =>
        Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
}

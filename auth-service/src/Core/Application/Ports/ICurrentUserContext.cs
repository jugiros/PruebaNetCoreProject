namespace AuthService.Application.Ports;

public interface ICurrentUserContext
{
    string UserId { get; }
    string Email { get; }
    IReadOnlyCollection<string> Scopes { get; }
    bool HasScope(string scope);
}

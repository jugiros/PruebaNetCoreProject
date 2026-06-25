namespace PruebaNetCoreProject.Application.Ports.Driven;

public interface IAccountWriteRepository
{
    Task<bool> ExistsAsync(Guid accountId, CancellationToken cancellationToken = default);
}

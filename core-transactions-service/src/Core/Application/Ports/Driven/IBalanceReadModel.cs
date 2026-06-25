namespace PruebaNetCoreProject.Application.Ports.Driven;

public interface IBalanceReadModel
{
    Task<TDocument?> GetByAccountIdAsync<TDocument>(Guid accountId, CancellationToken cancellationToken = default);
    Task UpsertAsync<TDocument>(TDocument document, CancellationToken cancellationToken = default);
    Task<IEnumerable<TDocument>> GetPagedAsync<TDocument>(
        Guid accountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

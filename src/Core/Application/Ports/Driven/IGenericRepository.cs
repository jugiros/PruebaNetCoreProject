using PruebaNetCoreProject.Domain.Common;

namespace PruebaNetCoreProject.Application.Ports.Driven;

public interface IGenericRepository<T, TId>
    where T : Entity<TId>
    where TId : struct, IEquatable<TId>
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}

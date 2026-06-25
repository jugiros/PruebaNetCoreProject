using PruebaNetCoreProject.Application.Ports.Driven;
using PruebaNetCoreProject.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace PruebaNetCoreProject.Infrastructure.Persistence.Repositories;

public abstract class GenericRepository<T, TId> : IGenericRepository<T, TId>
    where T : Entity<TId>
    where TId : struct, IEquatable<TId>
{
    protected readonly DbContext DbContext;
    protected readonly DbSet<T> DbSet;

    protected GenericRepository(DbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = dbContext.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }
}

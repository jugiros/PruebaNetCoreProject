namespace PruebaNetCoreProject.Application.Common;

public interface IQueryHandler<TQuery, TResult>
    where TQuery : class
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

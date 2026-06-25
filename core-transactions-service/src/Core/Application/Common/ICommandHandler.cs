namespace PruebaNetCoreProject.Application.Common;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : class
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

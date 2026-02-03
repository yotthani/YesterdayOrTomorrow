namespace StarTrekGame.Application.Interfaces;

/// <summary>
/// Marker interface for commands (write operations).
/// </summary>
public interface ICommand<TResult>
{
}

/// <summary>
/// Marker interface for queries (read operations).
/// </summary>
public interface IQuery<TResult>
{
}

/// <summary>
/// Handler for commands.
/// </summary>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for queries.
/// </summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dispatches commands and queries to their handlers.
/// </summary>
public interface IDispatcher
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}

// Result and Result<T> are defined in StarTrekGame.Domain.SharedKernel
// Use: using StarTrekGame.Domain.SharedKernel;

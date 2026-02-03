namespace StarTrekGame.Domain.SharedKernel;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
public interface IRepository<T, TId> where T : AggregateRoot<TId> where TId : notnull
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience interface for repositories with Guid identifiers.
/// </summary>
public interface IRepository<T> : IRepository<T, Guid> where T : AggregateRoot<Guid>
{
}

/// <summary>
/// Unit of work pattern for transactional consistency.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

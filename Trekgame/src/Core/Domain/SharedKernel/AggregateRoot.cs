namespace StarTrekGame.Domain.SharedKernel;

/// <summary>
/// Base class for aggregate roots - the entry point for aggregate operations.
/// Aggregates are consistency boundaries in the domain.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    public int Version { get; protected set; } = 0;

    protected void IncrementVersion()
    {
        Version++;
        ModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Convenience class for aggregates with Guid identifiers.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
    }
}

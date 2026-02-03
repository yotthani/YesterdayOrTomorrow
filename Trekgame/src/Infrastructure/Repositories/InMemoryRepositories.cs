using StarTrekGame.Application.Interfaces;
using StarTrekGame.Application.UseCases.Game;
using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Infrastructure.Repositories;

/// <summary>
/// In-memory repository for development and testing.
/// Replace with EF Core implementation for production.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly Dictionary<Guid, T> _store = new();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<T>>(_store.Values.ToList());
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _store.Remove(entity.Id);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory star system repository with spatial queries.
/// </summary>
public class InMemoryStarSystemRepository : InMemoryRepository<StarSystem>
{
    public Task<List<StarSystem>> GetInRangeAsync(
        GalacticCoordinates center,
        double range,
        CancellationToken cancellationToken = default)
    {
        var systems = _store.Values
            .Where(s => s.Coordinates.DistanceTo(center) <= range)
            .ToList();
        return Task.FromResult(systems);
    }

    public Task<List<StarSystem>> GetByEmpireAsync(
        Guid empireId,
        CancellationToken cancellationToken = default)
    {
        var systems = _store.Values
            .Where(s => s.ControllingEmpireId == empireId)
            .ToList();
        return Task.FromResult(systems);
    }

    public Task<List<StarSystem>> GetExploredByEmpireAsync(
        Guid empireId,
        CancellationToken cancellationToken = default)
    {
        // This would need to be tracked separately in a real implementation
        var systems = _store.Values
            .Where(s => s.IsExplored)
            .ToList();
        return Task.FromResult(systems);
    }
}

/// <summary>
/// In-memory empire repository.
/// </summary>
public class InMemoryEmpireRepository : InMemoryRepository<Empire>
{
    public Task<Empire?> GetByPlayerAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        var empire = _store.Values
            .FirstOrDefault(e => e.PlayerId == playerId);
        return Task.FromResult(empire);
    }
}

/// <summary>
/// In-memory fleet repository.
/// </summary>
public class InMemoryFleetRepository : InMemoryRepository<Fleet>
{
    public Task<List<Fleet>> GetByEmpireAsync(
        Guid empireId,
        CancellationToken cancellationToken = default)
    {
        var fleets = _store.Values
            .Where(f => f.EmpireId == empireId)
            .ToList();
        return Task.FromResult(fleets);
    }

    public Task<List<Fleet>> GetInSystemAsync(
        Guid systemId,
        CancellationToken cancellationToken = default)
    {
        var fleets = _store.Values
            .Where(f => f.CurrentSystemId == systemId)
            .ToList();
        return Task.FromResult(fleets);
    }
}

/// <summary>
/// In-memory game repository.
/// </summary>
public class InMemoryGameRepository : IGameRepository
{
    private readonly Dictionary<Guid, GameInstance> _games = new();

    public Task<GameInstance?> GetAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

    public Task SaveAsync(GameInstance game, CancellationToken cancellationToken = default)
    {
        _games[game.Id] = game;
        return Task.CompletedTask;
    }

    public Task<List<GameInstance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_games.Values.ToList());
    }
}

/// <summary>
/// In-memory unit of work.
/// </summary>
public class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

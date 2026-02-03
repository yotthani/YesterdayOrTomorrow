using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    
    // Core
    public DbSet<GameSessionEntity> Games => Set<GameSessionEntity>();
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    
    // Political
    public DbSet<FactionEntity> Factions => Set<FactionEntity>();
    public DbSet<HouseEntity> Houses => Set<HouseEntity>();
    
    // Galaxy
    public DbSet<StarSystemEntity> Systems => Set<StarSystemEntity>();
    public DbSet<PlanetEntity> Planets => Set<PlanetEntity>();
    public DbSet<HyperlaneEntity> Hyperlanes => Set<HyperlaneEntity>();
    public DbSet<AnomalyEntity> Anomalies => Set<AnomalyEntity>();
    
    // Colonies & Population
    public DbSet<ColonyEntity> Colonies => Set<ColonyEntity>();
    public DbSet<PopEntity> Pops => Set<PopEntity>();
    public DbSet<BuildingEntity> Buildings => Set<BuildingEntity>();
    public DbSet<BuildQueueItemEntity> BuildQueues => Set<BuildQueueItemEntity>();
    public DbSet<OrbitalEntity> Orbitals => Set<OrbitalEntity>();
    
    // Military
    public DbSet<FleetEntity> Fleets => Set<FleetEntity>();
    public DbSet<ShipEntity> Ships => Set<ShipEntity>();
    
    // Trade & Transport
    public DbSet<TradeRouteEntity> TradeRoutes => Set<TradeRouteEntity>();
    
    // Technology
    public DbSet<TechnologyEntity> Technologies => Set<TechnologyEntity>();
    
    // Diplomacy
    public DbSet<DiplomaticRelationEntity> DiplomaticRelations => Set<DiplomaticRelationEntity>();
    
    // Espionage
    public DbSet<AgentEntity> Agents => Set<AgentEntity>();
    
    // Events
    public DbSet<GameEventEntity> GameEvents => Set<GameEventEntity>();
    
    public DbSet<TurnOrderEntity> TurnOrders => Set<TurnOrderEntity>();
    public DbSet<SaveGameEntity> SaveGames => Set<SaveGameEntity>();
    
    // Alias for code using StarSystems
    public DbSet<StarSystemEntity> StarSystems => Systems;
    // Knowledge
    public DbSet<KnownSystemEntity> KnownSystems => Set<KnownSystemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ═══════════════════════════════════════════════════════════════════
        // GAME SESSION
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<GameSessionEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.MarketPrices);
            e.HasMany(x => x.Factions).WithOne(x => x.Game).HasForeignKey(x => x.GameId);
            e.HasMany(x => x.StarSystems).WithOne(x => x.Game).HasForeignKey(x => x.GameId);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // FACTION & HOUSE
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<FactionEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.Treasury, t =>
            {
                t.OwnsOne(y => y.Primary);
                t.OwnsOne(y => y.Strategic);
                t.OwnsOne(y => y.Research);
            });
            e.HasMany(x => x.Houses).WithOne(x => x.Faction).HasForeignKey(x => x.FactionId);
            e.HasMany(x => x.Technologies).WithOne(x => x.Faction).HasForeignKey(x => x.FactionId);
            e.HasMany(x => x.KnownSystems).WithOne(x => x.Faction).HasForeignKey(x => x.FactionId);
            e.HasMany(x => x.Agents).WithOne(x => x.Faction).HasForeignKey(x => x.FactionId);
        });
        
        modelBuilder.Entity<HouseEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.Treasury, t =>
            {
                t.OwnsOne(y => y.Primary);
                t.OwnsOne(y => y.Strategic);
                t.OwnsOne(y => y.Research);
            });
            e.HasMany(x => x.Colonies).WithOne(x => x.House).HasForeignKey(x => x.HouseId);
            e.HasMany(x => x.Fleets).WithOne(x => x.House).HasForeignKey(x => x.HouseId);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // STAR SYSTEMS & PLANETS
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<StarSystemEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Planets).WithOne(x => x.System).HasForeignKey(x => x.SystemId);
            e.HasMany(x => x.Orbitals).WithOne(x => x.System).HasForeignKey(x => x.SystemId);
            e.HasMany(x => x.Fleets).WithOne(x => x.CurrentSystem).HasForeignKey(x => x.CurrentSystemId);
        });
        
        modelBuilder.Entity<PlanetEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Colony).WithOne(x => x.Planet).HasForeignKey<ColonyEntity>(x => x.PlanetId);
        });
        
        modelBuilder.Entity<HyperlaneEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.FromSystem).WithMany().HasForeignKey(x => x.FromSystemId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ToSystem).WithMany().HasForeignKey(x => x.ToSystemId).OnDelete(DeleteBehavior.Restrict);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // COLONIES & POPULATION
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<ColonyEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Pops).WithOne(x => x.Colony).HasForeignKey(x => x.ColonyId);
            e.HasMany(x => x.Buildings).WithOne(x => x.Colony).HasForeignKey(x => x.ColonyId);
            e.HasMany(x => x.BuildQueue).WithOne(x => x.Colony).HasForeignKey(x => x.ColonyId);
        });
        
        modelBuilder.Entity<PopEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.HomeColony).WithMany().HasForeignKey(x => x.HomeColonyId).OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<BuildingEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // MILITARY
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<FleetEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Ships).WithOne(x => x.Fleet).HasForeignKey(x => x.FleetId);
            e.HasOne(x => x.DestinationSystem).WithMany().HasForeignKey(x => x.DestinationSystemId).OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<ShipEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // TRADE ROUTES
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<TradeRouteEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.SourceSystem).WithMany().HasForeignKey(x => x.SourceSystemId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.DestinationSystem).WithMany().HasForeignKey(x => x.DestinationSystemId).OnDelete(DeleteBehavior.Restrict);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // DIPLOMACY
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<DiplomaticRelationEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.OtherFaction).WithMany().HasForeignKey(x => x.OtherFactionId).OnDelete(DeleteBehavior.Restrict);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<GameEventEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });
        
        // ═══════════════════════════════════════════════════════════════════
        // KNOWLEDGE
        // ═══════════════════════════════════════════════════════════════════
        
        modelBuilder.Entity<KnownSystemEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.System).WithMany().HasForeignKey(x => x.SystemId);
        });
        
        modelBuilder.Entity<AnomalyEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.System).WithMany().HasForeignKey(x => x.SystemId);
        });
    }
}

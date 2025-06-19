namespace SmartHome.Infrastructure;

public class SmartHomeContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users { get; set; }

    public DbSet<Broadcast> Broadcasts { get; set; }

    private readonly IMediator _mediator;

    public SmartHomeContext(DbContextOptions<SmartHomeContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEnityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BroadcastEntityTypeConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(Entity)))
            {
                entityType.AddIgnored(nameof(Entity.DomainEvents));
            }
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker.Entries<Entity>()
            .Where(o => o.Entity.DomainEvents.Any());

        var domainEvents = domainEntities.SelectMany(o => o.Entity.DomainEvents).ToList();

        foreach (var domainEntity in domainEntities)
        {
            domainEntity.Entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        await base.SaveChangesAsync(cancellationToken);

        return true;
    }
}

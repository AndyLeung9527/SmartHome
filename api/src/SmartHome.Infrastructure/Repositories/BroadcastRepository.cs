namespace SmartHome.Infrastructure.Repositories;

public class BroadcastRepository : IBroadcastRepository
{
    private readonly SmartHomeContext _smartHomeContext;

    public IUnitOfWork UnitOfWork => _smartHomeContext;

    public BroadcastRepository(SmartHomeContext smartHomeContext)
    {
        _smartHomeContext = smartHomeContext ?? throw new ArgumentNullException(nameof(smartHomeContext));
    }

    public Broadcast Add(Broadcast broadcast)
    {
        return _smartHomeContext.Broadcasts.Add(broadcast).Entity;
    }

    public void Update(Broadcast broadcast)
    {
        _smartHomeContext.Entry(broadcast).State = EntityState.Modified;
    }

    public Task<Broadcast?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Broadcasts.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<List<Broadcast>> GetAllAsync(DateTimeOffset earliestDateTime, CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Broadcasts.Where(o => o.CreatedAt >= earliestDateTime && o.Valid && o.ExpirationDate > DateTimeOffset.Now).ToListAsync(cancellationToken);
    }

    public Task<List<Broadcast>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Broadcasts.Where(o => o.Valid && o.ExpirationDate > DateTimeOffset.Now).ToListAsync(cancellationToken);
    }
}

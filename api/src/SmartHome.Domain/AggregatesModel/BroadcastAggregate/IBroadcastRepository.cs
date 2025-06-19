namespace SmartHome.Domain.AggregatesModel.BroadcastAggregate;

public interface IBroadcastRepository : IRepository<Broadcast>
{
    Broadcast Add(Broadcast broadcast);

    void Update(Broadcast broadcast);

    Task<Broadcast?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<List<Broadcast>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<Broadcast>> GetAllAsync(DateTimeOffset earliestDateTime, CancellationToken cancellationToken = default);
}

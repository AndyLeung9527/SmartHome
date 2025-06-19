namespace SmartHome.Domain.AggregatesModel.BroadcastAggregate;

public class Broadcast : Entity<long>, IAggregateRoot
{
    public long UserId { get; protected set; }

    public string? Message { get; protected set; }

    public bool Valid { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset ExpirationDate { get; protected set; }

    protected Broadcast() : base(default) { }

    public Broadcast(long id, long userId, string message, DateTimeOffset expirationDate) : base(id)
    {
        UserId = userId;
        Message = message;
        ExpirationDate = expirationDate;
        Valid = true;
        CreatedAt = DateTimeOffset.Now;

        AddDomainEvent(new BroadcastCreatedDomainEvent(this));
    }
}

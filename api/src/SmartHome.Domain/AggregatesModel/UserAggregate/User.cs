namespace SmartHome.Domain.AggregatesModel.UserAggregate;

public class User : Entity<long>, IAggregateRoot
{
    public string? Name { get; protected set; }

    public string? Email { get; protected set; }

    public DateTimeOffset DateOfBirth { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset LastLoginAt { get; protected set; }

    public DateTimeOffset LastReadBroadcastAt { get; protected set; }

    protected User() : base(default) { }

    public User(long id, string? name, string? email, DateTimeOffset dateOfBirth) : base(id)
    {
        Name = name;
        Email = email;
        DateOfBirth = dateOfBirth;
        CreatedAt = DateTimeOffset.Now;
        LastLoginAt = DateTimeOffset.Now;
        LastReadBroadcastAt = DateTimeOffset.Now;

        AddDomainEvent(new UserCreatedDomainEvent(this));
    }

    public void UpdateLastLoginAt()
    {
        LastLoginAt = DateTimeOffset.Now;
    }

    public void UpdateLastReadBroadcastAt()
    {
        LastReadBroadcastAt = DateTimeOffset.Now;
    }
}

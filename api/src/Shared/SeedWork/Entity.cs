namespace Shared;

public abstract class Entity<TKey> where TKey : struct, IEquatable<TKey>
{
    private int? _requestedHashCode;
    private List<INotification>? _domainEvents;

    public virtual TKey Id { get; protected set; }

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly() ?? ReadOnlyCollection<INotification>.Empty;

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            if (!_requestedHashCode.HasValue)
            {
                // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)
                _requestedHashCode = Id.GetHashCode() ^ 31;
            }

            return _requestedHashCode.Value;
        }

        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || !(obj is Entity<TKey>))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (GetType() != obj.GetType())
        {
            return false;
        }

        Entity<TKey> item = (Entity<TKey>)obj;

        if (item.IsTransient() || IsTransient())
        {
            return false;
        }

        return item.Id.Equals(Id);
    }

    public static bool operator ==(Entity<TKey> left, Entity<TKey> right)
    {
        if (Equals(left, null))
        {
            return Equals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TKey> left, Entity<TKey> right)
    {
        return !(left == right);
    }

    public bool IsTransient()
    {
        return Id.Equals(default);
    }

    public void AddDomainEvent(INotification notification)
    {
        _domainEvents = _domainEvents ?? new List<INotification>();
        _domainEvents.Add(notification);
    }

    public void RemoveDomainEvent(INotification notification)
    {
        _domainEvents?.Remove(notification);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
        _domainEvents = null;
    }
}

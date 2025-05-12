namespace SmartHome.Domain.AggregatesModel.UserAggregate;

public class User : Entity<Guid>, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTimeOffset DateOfBirth { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
}

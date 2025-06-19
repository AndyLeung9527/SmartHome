namespace SmartHome.Domain.Events;

public record UserCreatedDomainEvent(User User) : INotification;

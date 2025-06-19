namespace SmartHome.Domain.Events;

public record BroadcastCreatedDomainEvent(Broadcast Broadcast) : INotification;

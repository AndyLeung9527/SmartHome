namespace SmartHome.Api.Application.DomainEventHandlers;

public class BroadcastCreatedDomainEventHandler(ILogger<BroadcastCreatedDomainEventHandler> logger, IHubContext<BroadcastHub> broadcastHub)
    : INotificationHandler<BroadcastCreatedDomainEvent>
{
    public async Task Handle(BroadcastCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await broadcastHub.Clients.All.SendAsync("broadcastReceived", notification.Broadcast.Message);
    }
}

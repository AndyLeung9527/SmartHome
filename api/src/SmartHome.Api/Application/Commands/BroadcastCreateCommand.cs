namespace SmartHome.Api.Application.Commands;

public record BroadcastCreateCommand(long UserId, string Message) : IRequest<(bool, string)>;

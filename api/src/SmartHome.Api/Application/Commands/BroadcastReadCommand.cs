namespace SmartHome.Api.Application.Commands;

public record BroadcastReadCommand(long UserId) : IRequest<(bool, string, List<BroadcastResponse>)>;

namespace SmartHome.Api.Dtos;

public record BroadcastResponse(string? PublishUserName, string? Message, DateTimeOffset CreatedAt, bool IsRead);

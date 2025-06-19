namespace SmartHome.Api.Application.Commands;

public record UserLoggedInCommand(long Id, string? Name, string? Email, DateTimeOffset dateOfBirth) : IRequest<bool>;

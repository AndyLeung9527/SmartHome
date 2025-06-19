namespace SmartHome.Api.Dtos;

/// <summary>
/// 用户信息
/// </summary>
/// <param name="Id">用户id</param>
/// <param name="Name">用户名</param>
/// <param name="email">邮箱</param>
/// <param name="dateOfBirth">出生日期</param>
/// <param name="roles">角色</param>
public record UserInfoResponse(string? Id, string? Name, string? email, DateTimeOffset dateOfBirth, IEnumerable<string> roles);
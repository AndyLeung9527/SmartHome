namespace Identity.Api.Dtos;

/// <summary>
/// 获取用户名、访问令牌和刷新令牌响应对象
/// </summary>
/// <param name="Name">用户名</param>
/// <param name="AccessToken">访问令牌</param>
/// <param name="RefreshToken">刷新令牌</param>
public record JwtTokenResponse(string Name, string AccessToken, string RefreshToken);
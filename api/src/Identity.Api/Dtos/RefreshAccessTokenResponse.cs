namespace Identity.Api.Dtos;

/// <summary>
/// 刷新访问令牌响应对象
/// </summary>
/// <param name="AccessToken">访问令牌</param>
public record RefreshAccessTokenResponse(string AccessToken);
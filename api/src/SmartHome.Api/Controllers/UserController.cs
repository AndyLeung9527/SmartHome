namespace SmartHome.Api.Controllers;

/// <summary>
/// 用户管理
/// </summary>
[Authorize]
[Route("[controller]/[action]")]
[ApiController]
public class UserController : ControllerBase
{
    /// <summary>
    /// 获取用户信息
    /// </summary>
    [HttpGet]
    public Task<Results<Ok<UserInfoResponse>, BadRequest<string>>> Info()
    {
        var name = User.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value;

        return Task.FromResult<Results<Ok<UserInfoResponse>, BadRequest<string>>>(TypedResults.Ok(new UserInfoResponse(name ?? string.Empty)));
    }
}

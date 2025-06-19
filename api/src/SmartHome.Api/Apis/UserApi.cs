namespace SmartHome.Api.Apis;

public static class UserApi
{
    public static RouteGroupBuilder MapUserApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.NewVersionedApi("User")
            .MapGroup("api/v{apiVersion:apiVersion}/user")
            .HasApiVersion(1)
            .RequireAuthorization();

        api.MapGet("/info", InfoAsync);

        return api;
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    // 当只注入HttpContext时，此api则不被OpenApi发现
    public static async Task<Results<Ok<UserInfoResponse>, BadRequest<string>, ProblemHttpResult>> InfoAsync(ClaimsPrincipal user, [FromServices] IMediator mediator)
    {
        var id = user.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
        var name = user.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value;
        var email = user.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
        var dateOfBirthStr = user.Claims.FirstOrDefault(o => o.Type == ClaimTypes.DateOfBirth)?.Value;
        var roles = user.Claims.Where(o => o.Type == ClaimTypes.Role).Select(o => o.Value);

        if (!long.TryParse(id, out var userId))
        {
            return TypedResults.BadRequest("用户id不符规则，请联系管理员");
        }
        DateTimeOffset.TryParse(dateOfBirthStr, out var dateOfBirth);

        var request = new UserLoggedInCommand(userId, name, email, dateOfBirth);
        var commandResult = await mediator.Send(request);
        if (!commandResult)
        {
            return TypedResults.Problem("用户登录信息更新失败，请联系管理员", statusCode: (int)HttpStatusCode.InternalServerError);
        }

        return TypedResults.Ok(new UserInfoResponse(id, name, email, dateOfBirth, roles));
    }
}

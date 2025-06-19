namespace SmartHome.Api.Apis;

public static class BroadcastApi
{
    public static RouteGroupBuilder MapBroadcastApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.NewVersionedApi("Broadcast")
            .MapGroup("api/v{apiVersion:apiVersion}/broadcast")
            .HasApiVersion(1)
            .RequireAuthorization();

        api.MapGet("/", GetAllAsync).RequireAuthorization();
        api.MapPost("/", PublishAsync).RequireAuthorization(builder =>
        {
            builder.RequireRole(RoleConsts.AdministratorRoleName);
        });

        return api;
    }

    /// <summary>
    /// 获取所有广播
    /// </summary>
    public static async Task<Results<Ok<List<BroadcastResponse>>, BadRequest<string>>> GetAllAsync(ClaimsPrincipal user, [FromServices] IMediator mediator)
    {
        var id = user.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(id, out var userId))
        {
            return TypedResults.BadRequest("用户id不符规则，请联系管理员");
        }

        var request = new BroadcastReadCommand(userId);
        var commandResult = await mediator.Send(request);
        if (!commandResult.Item1)
        {
            return TypedResults.BadRequest(commandResult.Item2);
        }
        return TypedResults.Ok(commandResult.Item3);
    }

    /// <summary>
    /// 发布
    /// </summary>
    public static async Task<Results<Ok, BadRequest<string>>> PublishAsync(ClaimsPrincipal user, [FromServices] IMediator mediator, [FromBody] BroadcastDto dto)
    {
        var id = user.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(id, out var userId))
        {
            return TypedResults.BadRequest("用户id不符规则，请联系管理员");
        }
        if (string.IsNullOrWhiteSpace(dto.Message) || dto.Message.Length > 500)
        {
            return TypedResults.BadRequest("消息内容不能为空且不能超过500个字符");
        }

        var request = new BroadcastCreateCommand(userId, dto.Message);
        var commandResult = await mediator.Send(request);
        if (!commandResult.Item1)
        {
            return TypedResults.BadRequest(commandResult.Item2);
        }
        return TypedResults.Ok();
    }
}

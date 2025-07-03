namespace SmartHome.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddSmartHomeAuthentication();
        builder.AllowAllCors();
        builder.AddEventBus();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
        });
        builder.Services.AddIdGen(builder.Configuration.GetSection("App").GetValue<int>("WorkerId"));
        builder.Services.AddSignalR();
        builder.AddWebApiVersioning();
        builder.AddSmartHomeDbContext();

        var app = builder.Build();

        app.UseRouting();
        app.UseCors(CorsExtensions.AllowAllCorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapHub<BroadcastHub>("/broadcastHub");

        app.MapUserApiV1();
        app.MapBroadcastApiV1();

        app.Run();
    }
}

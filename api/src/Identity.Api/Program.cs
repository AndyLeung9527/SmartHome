namespace Identity.Api;

public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        builder.AllowAllCors();
        builder.AddIdentityServices();
        builder.AddMvcVersioning();
        builder.AddRsaServices();
        builder.AddApplicationDbContext();
        builder.Services.AddOptions().Configure<AppOptions>(options => builder.Configuration.GetSection("App").Bind(options));
        builder.Services.AddOptions().Configure<JwtOptions>(options => builder.Configuration.GetSection("Jwt").Bind(options));
        builder.Services.AddSingleton<RedisService>();
        builder.Services.AddIdGen(builder.Configuration.GetSection("App").GetValue<int>("WorkerId"));

        var app = builder.Build();

        app.UseCors(CorsExtensions.AllowAllCorsPolicyName);
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

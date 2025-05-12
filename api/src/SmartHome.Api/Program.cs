namespace SmartHome.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddSmartHomeAuthentication();
        builder.AllowAllCors();

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseRouting();
        app.UseCors(CorsExtensions.AllowAllCorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

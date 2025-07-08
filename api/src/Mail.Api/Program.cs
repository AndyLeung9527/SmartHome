namespace Mail.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddMailAuthentication();
        builder.AddUnencryptedGrpc();
        builder.Services.AddAuthorization();
        builder.Services.AddOptions().Configure<EmailOption>(builder.Configuration.GetSection("Email"));

        var app = builder.Build();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGrpcService<Services.MailService>();

        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

        app.Run();
    }
}

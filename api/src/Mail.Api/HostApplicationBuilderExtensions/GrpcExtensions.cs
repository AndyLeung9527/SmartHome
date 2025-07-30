namespace Mail.Api.HostApplicationBuilderExtensions;

public static class GrpcExtensions
{
    public static IWebHostBuilder AddUnencryptedGrpc(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        return builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(options =>
            {
                options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        });
    }
}

namespace Mail.Api.HostApplicationBuilderExtensions;

public static class GrpcExtensions
{
    public static IWebHostBuilder AddUnencryptedGrpc(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        return builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(10105, listenOptions =>
            {
                // Configure Kestrel to use HTTP/2 without TLS for gRPC
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        });
    }
}

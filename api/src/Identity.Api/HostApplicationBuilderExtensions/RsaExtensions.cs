namespace Identity.Api.HostApplicationBuilderExtensions;

public static class RsaExtensions
{
    public static IHostApplicationBuilder AddRsaServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<RsaKeyService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<RsaKeyService>());

        return builder;
    }
}


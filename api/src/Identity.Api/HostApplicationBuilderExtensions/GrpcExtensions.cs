namespace Identity.Api.HostApplicationBuilderExtensions;

public static class GrpcExtensions
{
    public static IHostApplicationBuilder AddMailService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddGrpcClient<Protos.MailService.MailServiceClient>(options =>
        {
            options.Address = new Uri(builder.Configuration.GetConnectionString("MailService") ?? string.Empty);
        });
        builder.Services.AddSingleton<Services.MailService>();
        return builder;
    }
}

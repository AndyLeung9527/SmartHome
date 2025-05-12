namespace SmartHome.Api.HostApplicationBuilderExtensions;

public static class CorsExtensions
{
    public static string AllowAllCorsPolicyName => "AllowAll";

    public static IHostApplicationBuilder AllowAllCors(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(AllowAllCorsPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return builder;
    }
}
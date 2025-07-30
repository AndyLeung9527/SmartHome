namespace Identity.Api.HostApplicationBuilderExtensions;

public static class ApplicationDbContextExtensions
{
    public static IHostApplicationBuilder AddApplicationDbContext(this IHostApplicationBuilder builder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb"));
        });

        return builder;
    }
}

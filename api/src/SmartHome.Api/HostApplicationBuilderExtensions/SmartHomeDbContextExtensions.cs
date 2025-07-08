namespace SmartHome.Api.HostApplicationBuilderExtensions;

public static class SmartHomeDbContextExtensions
{
    public static IHostApplicationBuilder AddSmartHomeDbContext(this IHostApplicationBuilder builder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        builder.Services.AddDbContext<SmartHomeContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql"));
        });

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IBroadcastRepository, BroadcastRepository>();

        builder.Services.AddMigration<SmartHomeContext>();

        return builder;
    }
}

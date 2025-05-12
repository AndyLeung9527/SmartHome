using Role = Identity.Api.Models.Role;

namespace Identity.Api.HostApplicationBuilderExtensions;

public static class IdentityExtensions
{
    public static IHostApplicationBuilder AddIdentityServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddIdentity<User, Role>(options =>
        {
            builder.Configuration.GetSection("Identity").Bind(options);
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(2);
        });

        return builder;
    }
}

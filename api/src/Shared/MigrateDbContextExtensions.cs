using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Hosting;

internal static class MigrateDbContextExtensions
{
    private static readonly string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
    where TContext : DbContext
    where TDbSeeder : class, IDbSeeder<TContext>
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDbSeeder<TContext>, TDbSeeder>());

        return services.AddMigration<TContext>();
    }

    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        return services.AddHostedService<MigrationHostedService<TContext>>();
    }

    private class MigrationHostedService<TContext>(IServiceProvider serviceProvider)
        : BackgroundService where TContext : DbContext
    {
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetRequiredService<TContext>();
            var seeders = services.GetServices<IDbSeeder<TContext>>();

            using var activity = ActivitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                var strategy = context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");
                    try
                    {
                        await context.Database.MigrateAsync(cancellationToken);
                        foreach (var seeder in seeders)
                        {
                            await seeder.SeedAsync(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        activity?.SetExceptionTags(ex);
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

                activity?.SetExceptionTags(ex);

                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
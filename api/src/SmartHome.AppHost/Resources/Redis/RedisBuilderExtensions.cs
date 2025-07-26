using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace SmartHome.AppHost.Resources.Redis;

public static class RedisBuilderExtensions
{
    public static IResourceBuilder<RedisResource> AddRedis(this IDistributedApplicationBuilder builder, [ResourceName] string name, int? port = null, IResourceBuilder<ParameterResource>? password = null)
    {
        var redis = new RedisResource(name, password?.Resource);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(redis, async (@event, ct) =>
        {
            connectionString = await redis.ConnectionStringExpression.GetValueAsync(ct);

            if (connectionString == null)
            {
                throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{redis.Name}' resource but the connection string was null.");
            }
        });

        var healthCheckKey = $"{name}_check";
        builder.Services.AddHealthChecks().AddRedis(sp => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
            name: healthCheckKey);

        return builder.AddResource(redis)
            .WithEndpoint(port: port, targetPort: 6379, name: RedisResource.PrimaryEndpointName)
            .WithImage("redis")
            .WithImageRegistry("docker.io")
            .WithEnvironment(context =>
            {
                if (redis.PasswordParameter is not null)
                {
                    context.EnvironmentVariables["REDIS_PASSWORD"] = redis.PasswordParameter;
                }
            })
            .WithArgs(context =>
            {
                var redisCommand = new List<string>
                          {
                              "redis-server"
                          };

                if (redis.PasswordParameter is not null)
                {
                    redisCommand.Add("--requirepass");
                    redisCommand.Add("$REDIS_PASSWORD");
                }

                if (redis.TryGetLastAnnotation<PersistenceAnnotation>(out var persistenceAnnotation))
                {
                    var interval = (persistenceAnnotation.Interval ?? TimeSpan.FromSeconds(60)).TotalSeconds.ToString(CultureInfo.InvariantCulture);

                    redisCommand.Add("--save");
                    redisCommand.Add(interval);
                    redisCommand.Add(persistenceAnnotation.KeysChangedThreshold.ToString(CultureInfo.InvariantCulture));
                }

                context.Args.Add("-c");
                context.Args.Add(string.Join(' ', redisCommand));
            })
            .WithHealthCheck(healthCheckKey)
            .WithEntrypoint("/bin/sh");
    }

    public static IResourceBuilder<RedisResource> WithDataVolume(this IResourceBuilder<RedisResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/data", isReadOnly);
        if (!isReadOnly)
        {
            builder.WithPersistence();
        }
        return builder;
    }

    public static IResourceBuilder<RedisResource> WithDataBindMount(this IResourceBuilder<RedisResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(source);

        builder.WithBindMount(source, "/data", isReadOnly);
        if (!isReadOnly)
        {
            builder.WithPersistence();
        }
        return builder;
    }

    public static IResourceBuilder<RedisResource> WithPersistence(this IResourceBuilder<RedisResource> builder, TimeSpan? interval = null, long keysChangedThreshold = 1)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithAnnotation(
            new PersistenceAnnotation(interval, keysChangedThreshold), ResourceAnnotationMutationBehavior.Replace);
    }

    private sealed record PersistenceAnnotation(TimeSpan? Interval, long KeysChangedThreshold) : IResourceAnnotation;
}

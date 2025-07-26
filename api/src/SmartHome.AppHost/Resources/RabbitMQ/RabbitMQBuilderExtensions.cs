using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace SmartHome.AppHost.Resources.RabbitMQ;

public static class RabbitMQBuilderExtensions
{
    public static IResourceBuilder<RabbitMQResource> AddRabbitMQ(this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        var passwordParameter = password?.Resource ??
            ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password", special: false);

        var rabbitMq = new RabbitMQResource(name, userName?.Resource, passwordParameter);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(rabbitMq, async (@event, ct) =>
        {
            connectionString = await rabbitMq.ConnectionStringExpression.GetValueAsync(ct);

            if (connectionString == null)
            {
                throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{rabbitMq.Name}' resource but the connection string was null.");
            }
        });

        var healthCheckKey = $"{name}_check";
        IConnection? connection = null;
        builder.Services.AddHealthChecks().AddRabbitMQ(async sp =>
        {
            if (connection == null)
            {
                var factory = new ConnectionFactory
                {
                    Uri = new(connectionString!)
                };
                connection = await factory.CreateConnectionAsync();
            }
            return connection;
        }, healthCheckKey);

        return builder.AddResource(rabbitMq)
            .WithEndpoint(port: port, targetPort: 5672, name: RabbitMQResource.PrimaryEndpointName)
            .WithImage("rabbitmq")
            .WithImageRegistry("docker.io")
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitMq.UserNameReference;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_PASS"] = rabbitMq.PasswordParameter;
            })
            .WithHealthCheck(healthCheckKey);
    }

    public static IResourceBuilder<RabbitMQResource> WithDataVolume(this IResourceBuilder<RabbitMQResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/var/lib/rabbitmq", isReadOnly)
            .RunWithStableNodeName();
    }

    public static IResourceBuilder<RabbitMQResource> WithDataBindMount(this IResourceBuilder<RabbitMQResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(source);

        return builder.WithBindMount(source, "/var/lib/rabbitmq", isReadOnly)
                      .RunWithStableNodeName();
    }

    private static IResourceBuilder<RabbitMQResource> RunWithStableNodeName(this IResourceBuilder<RabbitMQResource> builder)
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.WithEnvironment(context =>
            {
                var nodeName = $"{builder.Resource.Name}@localhost";
                context.EnvironmentVariables["RABBITMQ_NODENAME"] = nodeName;
            });
        }

        return builder;
    }
}

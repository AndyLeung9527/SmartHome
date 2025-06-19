namespace EventBusRabbitMQ;

public static class RabbitMqDependencyInjectionExtensions
{
    private const string ActivitySourceName = "EventBusRabbitMQ.Init";
    private static readonly ActivitySource s_activitySource = new ActivitySource(ActivitySourceName);

    public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder builder, string connectionName, Action<EventBusOptions> options)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            IConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri(connectionString);
            builder.Services.AddSingleton(factory);

            var resiliencePipelineBuilder = new ResiliencePipelineBuilder();
            resiliencePipelineBuilder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = static args => args.Outcome is { Exception: SocketException or BrokerUnreachableException }
                ? PredicateResult.True()
                : PredicateResult.False(),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1)
            });
            var resiliencePipeline = resiliencePipelineBuilder.Build();
            using var activity = s_activitySource.StartActivity("rabbitmq connect", ActivityKind.Client);
            AddRabbitMQTags(activity, factory.Uri);
            var connection = resiliencePipeline.ExecuteAsync(static async (factory, ct) =>
            {
                using var connectAttemptActivity = s_activitySource.StartActivity("rabbitmq connect attempt", ActivityKind.Client);
                AddRabbitMQTags(connectAttemptActivity, factory.Uri, "connect");

                try
                {
                    return await factory.CreateConnectionAsync(ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    AddRabbitMQExceptionTags(connectAttemptActivity, ex);
                    throw;
                }
            }, factory).AsTask().GetAwaiter().GetResult();
            builder.Services.AddSingleton(connection);
        }
        else
        {
            throw new InvalidOperationException($"Connection string '{connectionName}' not found in configuration.");
        }

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(ActivitySourceName)
                .AddSource("RabbitMQ.Client.*")
                .AddSource(RabbitMQTelemetry.ActivitySourceName);
            });

        builder.Services.AddOptions<EventBusOptions>().Configure(options);

        builder.Services.AddSingleton<RabbitMQTelemetry>();
        builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

        builder.Services.AddHostedService(sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    private static void AddRabbitMQTags(Activity? activity, Uri address, string? operation = null)
    {
        if (activity is null)
        {
            return;
        }

        activity.AddTag("server.address", address.Host);
        activity.AddTag("server.port", address.Port);
        activity.AddTag("messaging.system", "rabbitmq");
        if (operation is not null)
        {
            activity.AddTag("messaging.operation", operation);
        }
    }

    private static void AddRabbitMQExceptionTags(Activity? connectAttemptActivity, Exception ex)
    {
        if (connectAttemptActivity is null)
        {
            return;
        }

        connectAttemptActivity.AddTag("exception.message", ex.Message);
        connectAttemptActivity.AddTag("exception.stacktrace", ex.ToString());
        connectAttemptActivity.AddTag("exception.type", ex.GetType().FullName);
        connectAttemptActivity.SetStatus(ActivityStatusCode.Error);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}

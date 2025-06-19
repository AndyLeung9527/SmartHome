namespace EventBusRabbitMQ;

public sealed class RabbitMQEventBus(
    ILogger<RabbitMQEventBus> logger,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions,
    RabbitMQTelemetry rabbitMQTelemetry
    ) : IEventBus, IDisposable, IHostedService
{
    private const string ExchangeName = "smart_home_event_bus";

    private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(options.Value.RetryCount);
    private readonly TextMapPropagator _propagator = rabbitMQTelemetry.Propagator;
    private readonly ActivitySource _activitySource = rabbitMQTelemetry.ActivitySource;
    private readonly string _queueName = options.Value.SubscriptionClientName;
    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOptions.Value;

    private IConnection? _rabbitMQConnection;
    private IChannel? _consumerChannel;

    public async Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, routingKey);
        }

        using var channel = await (_rabbitMQConnection?.CreateChannelAsync() ?? throw new InvalidOperationException("RabbitMQ connection is not open"));

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);
        }

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);

        var body = SerializeMessage(@event);

        var activityName = $"{routingKey} publish";

        await _pipeline.ExecuteAsync(async ct =>
        {
            using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client);

            ActivityContext contextToInject = default;

            if (activity is not null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current is not null)
            {
                contextToInject = Activity.Current.Context;
            }

            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            _propagator.Inject(
                context: new PropagationContext(contextToInject, Baggage.Current),
                carrier: properties,
                setter: (props, key, value) =>
                {
                    props.Headers ??= new Dictionary<string, object?>();
                    props.Headers[key] = value;
                });

            SetActivityContext(activity, routingKey, "publish");

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
            }

            try
            {
                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                activity?.SetExceptionTags(ex);
                throw;
            }
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Factory.StartNew(async () =>
        {
            try
            {
                logger.LogInformation("Starting RabbitMQ connection on a background thread");

                _rabbitMQConnection = serviceProvider.GetRequiredService<IConnection>();
                if (!_rabbitMQConnection.IsOpen)
                {
                    return;
                }

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Creating RabbitMQ consumer channel");
                }

                _consumerChannel = await _rabbitMQConnection.CreateChannelAsync();

                _consumerChannel.CallbackExceptionAsync += (sender, ea) =>
                {
                    logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                    return Task.CompletedTask;
                };

                await _consumerChannel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);

                await _consumerChannel.QueueDeclareAsync(
                    queue: _queueName,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Starting RabbitMQ basic consume");
                }

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    var parentContext = _propagator.Extract(default, eventArgs.BasicProperties, (props, key) =>
                    {
                        if (props.Headers?.TryGetValue(key, out var value) ?? false)
                        {
                            var bytes = value as byte[];
                            return bytes is null ? [] : [Encoding.UTF8.GetString(bytes)];
                        }
                        return [];
                    });
                    Baggage.Current = parentContext.Baggage;

                    var activityName = $"{eventArgs.RoutingKey} receive";
                    using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client, parentContext.ActivityContext);

                    SetActivityContext(activity, eventArgs.RoutingKey, "receive");

                    var eventName = eventArgs.RoutingKey;
                    var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

                    try
                    {
                        activity?.SetTag("message", message);

                        if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
                        {
                            throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                        }

                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
                        }

                        await using var scope = serviceProvider.CreateAsyncScope();
                        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
                        {
                            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
                        }
                        else
                        {
                            var integrationEvent = DeserializeMessage(message, eventType);
                            foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventName))
                            {
                                await handler.HandleAsync(integrationEvent);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);

                        activity?.SetExceptionTags(ex);
                    }

                    await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                };

                await _consumerChannel.BasicConsumeAsync(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer
                    );

                foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    await _consumerChannel.QueueBindAsync(
                        queue: _queueName,
                        exchange: ExchangeName,
                        routingKey: eventName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting RabbitMQ connection");
            }
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
    Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
    Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent ?? new();
    }

    private static void SetActivityContext(Activity? activity, string routingKey, string operation)
    {
        if (activity is not null)
        {
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.destination_kind", "queue");
            activity.SetTag("messaging.operation", operation);
            activity.SetTag("messaging.destination.name", routingKey);
            activity.SetTag("messaging.rabbitmq.routing_key", routingKey);
        }
    }

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = context => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(Math.Pow(2, context.AttemptNumber)))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();
    }
}

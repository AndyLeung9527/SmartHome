namespace SmartHome.Api.HostApplicationBuilderExtensions;

public static class EventBusExtensions
{
    public static IHostApplicationBuilder AddEventBus(this IHostApplicationBuilder builder)
    {
        var eventBusBuilder = builder.AddRabbitMqEventBus("EventBus", options => builder.Configuration.GetSection("EventBus")?.Bind(options));

        return builder;
    }
}

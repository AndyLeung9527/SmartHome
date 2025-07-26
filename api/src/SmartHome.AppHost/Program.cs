using SmartHome.AppHost.Resources.Postgres;
using SmartHome.AppHost.Resources.RabbitMQ;
using SmartHome.AppHost.Resources.Redis;

namespace SmartHome.AppHost;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var pgUserName = builder.AddParameter("postgres-username");
        var pgPassword = builder.AddParameter("postgres-password");
        var postgres = builder.AddPostgres("postgres", pgUserName, pgPassword);

        var redis = builder.AddRedis("redis");

        var rqUserName = builder.AddParameter("rabbitmq-username");
        var rqPassword = builder.AddParameter("rabbitmq-password");
        var rabbitmq = builder.AddRabbitMQ("rabbitmq", rqUserName, rqPassword);

        builder.Build().Run();
    }
}
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

        var identityDb = postgres.AddDatabase("identitydb");
        var smartHomeDb = postgres.AddDatabase("smarthomedb");

        var redis = builder.AddRedis("redis");

        var rqUserName = builder.AddParameter("rabbitmq-username");
        var rqPassword = builder.AddParameter("rabbitmq-password");
        var rabbitmq = builder.AddRabbitMQ("eventbus", rqUserName, rqPassword);

        var launchProfileName = "http";

        var mailApi = builder.AddProject<Projects.Mail_Api>("mail-api", launchProfileName);

        var mailEndpoint = mailApi.GetEndpoint(launchProfileName);

        var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api", launchProfileName)
            .WithReference(identityDb).WaitFor(identityDb)
            .WithReference(redis).WaitFor(redis)
            .WithEnvironment("ConnectionStrings__MailService", mailEndpoint);

        var smartHomeApi = builder.AddProject<Projects.SmartHome_Api>("smarthome-api", launchProfileName)
            .WithReference(smartHomeDb).WaitFor(smartHomeDb)
            .WithReference(redis).WaitFor(redis)
            .WithReference(rabbitmq).WaitFor(rabbitmq);

        builder.Build().Run();
    }
}
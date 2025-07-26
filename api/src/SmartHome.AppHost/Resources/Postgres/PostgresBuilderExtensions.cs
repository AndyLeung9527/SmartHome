using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SmartHome.AppHost.Resources.Postgres;

public static class PostgresBuilderExtensions
{
    private const string UserEnvVarName = "POSTGRES_USER";
    private const string PasswordEnvVarName = "POSTGRES_PASSWORD";

    public static IResourceBuilder<PostgresResource> AddPostgres(this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var postgresResource = new PostgresResource(name, userName?.Resource, passwordParameter);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(postgresResource, async (@event, ct) =>
        {
            connectionString = await postgresResource.ConnectionStringExpression.GetValueAsync(ct);

            if (connectionString == null)
            {
                throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{postgresResource.Name}' resource but the connection string was null.");
            }
        });

        builder.Eventing.Subscribe<ResourceReadyEvent>(postgresResource, async (@event, ct) =>
        {
            if (connectionString is null)
            {
                throw new DistributedApplicationException($"ResourceReadyEvent was published for the '{postgresResource.Name}' resource but the connection string was null.");
            }

            using var npgsqlConnection = new NpgsqlConnection(connectionString + ";Database=postgres;");

            await npgsqlConnection.OpenAsync(ct);

            if (npgsqlConnection.State != System.Data.ConnectionState.Open)
            {
                throw new InvalidOperationException($"Could not open connection to '{postgresResource.Name}'");
            }

            foreach (var name in postgresResource.Databases.Keys)
            {
                if (builder.Resources.FirstOrDefault(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase)) is PostgresDatabaseResource postgresDatabase)
                {
                    var scriptAnnotation = postgresDatabase.Annotations.OfType<PostgresCreateDatabaseScriptAnnotation>().LastOrDefault();

                    var logger = @event.Services.GetRequiredService<ResourceLoggerService>().GetLogger(postgresDatabase.Parent);
                    logger.LogDebug("Creating database '{DatabaseName}'", postgresDatabase.DatabaseName);

                    try
                    {
                        var quotedDatabaseIdentifier = new NpgsqlCommandBuilder().QuoteIdentifier(postgresDatabase.DatabaseName);
                        using var command = npgsqlConnection.CreateCommand();
                        command.CommandText = scriptAnnotation?.Script ?? $"CREATE DATABASE {quotedDatabaseIdentifier}";
                        await command.ExecuteNonQueryAsync(ct);
                        logger.LogDebug("Database '{Database}' created successfully", postgresDatabase.DatabaseName);
                    }
                    catch (PostgresException pe) when (pe.SqlState == "42P04")
                    {
                        // 数据库已存在则忽略
                        logger.LogDebug("Database '{DatabaseName}' already exists", postgresDatabase.DatabaseName);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to create database '{DatabaseName}'", postgresDatabase.DatabaseName);
                    }
                }
            }
        });

        var healthCheckKey = $"{name}_check";
        builder.Services.AddHealthChecks().AddNpgSql(
            connectionStringFactory: sp => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
            name: healthCheckKey,
            configure: connection => connection.ConnectionString += ";Database=postgres;"
        );

        return builder.AddResource(postgresResource)
            .WithEndpoint(port: port, targetPort: 5432, name: PostgresResource.PrimaryEndpointName)
            .WithImage("postgres")
            .WithImageRegistry("docker.io")
            .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "scram-sha-256")
            .WithEnvironment("POSTGRES_INITDB_ARGS", "--auth-host=scram-sha-256 --auth-local=scram-sha-256")
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables[UserEnvVarName] = postgresResource.UserNameReference;
                context.EnvironmentVariables[PasswordEnvVarName] = postgresResource.PasswordParameter;
            })
            .WithHealthCheck(healthCheckKey);
    }

    public static IResourceBuilder<PostgresDatabaseResource> AddDatabase(this IResourceBuilder<PostgresResource> builder, [ResourceName] string name, string? databaseNamne = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        databaseNamne ??= name;

        var posgresDatabaseResource = new PostgresDatabaseResource(name, databaseNamne, builder.Resource);

        builder.Resource.AddDatabase(name, databaseNamne);

        string? connectionString = null;

        builder.ApplicationBuilder.Eventing.Subscribe<ConnectionStringAvailableEvent>(posgresDatabaseResource, async (@event, ct) =>
        {
            connectionString = await posgresDatabaseResource.ConnectionStringExpression.GetValueAsync(ct);

            if (connectionString == null)
            {
                throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{name}' resource but the connection string was null.");
            }
        });

        var healthCheckKey = $"{name}_check";
        builder.ApplicationBuilder.Services.AddHealthChecks().AddNpgSql(sp => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
            name: healthCheckKey);

        return builder.ApplicationBuilder
            .AddResource(posgresDatabaseResource)
            .WithHealthCheck(healthCheckKey);
    }

    public static IResourceBuilder<PostgresResource> WithDataVolume(this IResourceBuilder<PostgresResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/var/lib/postgresql/data", isReadOnly);
    }

    public static IResourceBuilder<PostgresResource> WithDataBindMount(this IResourceBuilder<PostgresResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(source);

        return builder.WithBindMount(source, "/var/lib/postgresql/data", isReadOnly);
    }

    public static IResourceBuilder<PostgresDatabaseResource> WithCreationScript(this IResourceBuilder<PostgresDatabaseResource> builder, string script)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(script);

        builder.WithAnnotation(new PostgresCreateDatabaseScriptAnnotation(script));

        return builder;
    }
}

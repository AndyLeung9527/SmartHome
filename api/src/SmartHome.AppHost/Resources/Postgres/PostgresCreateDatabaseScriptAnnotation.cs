namespace SmartHome.AppHost.Resources.Postgres;

public sealed class PostgresCreateDatabaseScriptAnnotation : IResourceAnnotation
{
    public string Script { get; }

    public PostgresCreateDatabaseScriptAnnotation(string script)
    {
        ArgumentNullException.ThrowIfNull(script);
        Script = script;
    }
}

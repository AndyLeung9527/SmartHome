using System.Data.Common;

namespace SmartHome.AppHost.Resources.Postgres;

public class PostgresDatabaseResource(string name, string databaseName, PostgresResource postgresParaentResource)
    : Resource(name), IResourceWithParent<PostgresResource>, IResourceWithConnectionString
{
    public PostgresResource Parent => postgresParaentResource ?? throw new ArgumentNullException(nameof(postgresParaentResource));

    public string DatabaseName => string.IsNullOrWhiteSpace(databaseName) ? throw new ArgumentNullException(nameof(databaseName)) : databaseName;

    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            var connectionStringBuilder = new DbConnectionStringBuilder
            {
                ["Database"] = DatabaseName
            };

            return ReferenceExpression.Create($"{Parent};{connectionStringBuilder.ToString()}");
        }
    }
}

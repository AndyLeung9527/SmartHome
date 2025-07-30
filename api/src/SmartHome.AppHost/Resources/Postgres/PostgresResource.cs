namespace SmartHome.AppHost.Resources.Postgres;

public sealed class PostgresResource : ContainerResource, IResourceWithConnectionString
{
    private const string DefaultUserName = "postgres";

    private readonly Dictionary<string, string> _databases = new(StringComparer.OrdinalIgnoreCase);

    private ReferenceExpression ConnectionString =>
        ReferenceExpression.Create($"Host={PrimaryEndpoint.Property(EndpointProperty.Host)};Port={PrimaryEndpoint.Property(EndpointProperty.Port)};Username={UserNameReference};Password={PasswordParameter}");

    public const string PrimaryEndpointName = "tcp";

    public EndpointReference PrimaryEndpoint { get; }

    public ParameterResource? UserNameParameter { get; set; }

    public ParameterResource PasswordParameter { get; set; }

    public IReadOnlyDictionary<string, string> Databases => _databases;

    public ReferenceExpression UserNameReference =>
        UserNameParameter is null ?
        ReferenceExpression.Create($"{DefaultUserName}") :
        ReferenceExpression.Create($"{UserNameParameter}");

    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
            {
                return connectionStringAnnotation.Resource.ConnectionStringExpression;
            }

            return ConnectionString;
        }
    }

    public PostgresResource(string name, ParameterResource? userName, ParameterResource password)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(password);

        PrimaryEndpoint = new(this, PrimaryEndpointName);
        UserNameParameter = userName;
        PasswordParameter = password;
    }

    public void AddDatabase(string name, string databseName)
    {
        _databases.TryAdd(name, databseName);
    }
}
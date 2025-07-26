namespace SmartHome.AppHost.Resources.RabbitMQ;

public class RabbitMQResource : ContainerResource, IResourceWithConnectionString
{
    private const string DefaultUserName = "guest";

    public const string PrimaryEndpointName = "tcp";

    public EndpointReference PrimaryEndpoint { get; }

    public ParameterResource? UserNameParameter { get; }

    public ParameterResource PasswordParameter { get; }

    public ReferenceExpression UserNameReference => UserNameParameter is null ?
        ReferenceExpression.Create($"{DefaultUserName}") :
        ReferenceExpression.Create($"{UserNameParameter}");

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"amqp://{UserNameReference}:{PasswordParameter}@{PrimaryEndpoint.Property(EndpointProperty.HostAndPort)}");

    public RabbitMQResource(string name, ParameterResource? userName, ParameterResource password)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(password);

        PrimaryEndpoint = new(this, PrimaryEndpointName);
        UserNameParameter = userName;
        PasswordParameter = password;
    }
}

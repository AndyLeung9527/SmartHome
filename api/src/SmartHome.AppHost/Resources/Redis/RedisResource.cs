namespace SmartHome.AppHost.Resources.Redis;

public class RedisResource : ContainerResource, IResourceWithConnectionString
{
    public const string PrimaryEndpointName = "tcp";

    public EndpointReference PrimaryEndpoint { get; }

    public ParameterResource? PasswordParameter { get; }

    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
            {
                return connectionStringAnnotation.Resource.ConnectionStringExpression;
            }

            var builder = new ReferenceExpressionBuilder();
            builder.Append($"{PrimaryEndpoint.Property(EndpointProperty.HostAndPort)}");

            if (PasswordParameter is not null)
            {
                builder.Append($",password={PasswordParameter}");
            }

            return builder.Build();
        }
    }

    public RedisResource(string name, ParameterResource? password = null)
        : base(name)
    {
        PrimaryEndpoint = new(this, PrimaryEndpointName);
        PasswordParameter = password;
    }
}

namespace SmartHome.Api.HostApplicationBuilderExtensions;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddSmartHomeAuthentication(this IHostApplicationBuilder builder)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var publicKeyPem = File.ReadAllText(Path.Combine(currentDir, "public.pem"));
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false))
                };
            });

        return builder;
    }
}

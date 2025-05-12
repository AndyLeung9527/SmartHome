namespace Identity.Api.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "Identity";
    public int AccessExpirationMinutes { get; set; } = 60;
    public int RefreshExpirationMinutes { get; set; } = 420;
}

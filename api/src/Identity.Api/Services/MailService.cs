namespace Identity.Api.Services;

public sealed class MailService
{
    private readonly IServiceProvider _serviceProvider;

    private DateTime _expires = DateTime.MinValue;
    private string _jwtToken = string.Empty;

    public MailService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<(bool, string)> SendEmailAsync(string toName, string toEmail, string subject, string body)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var mailService = scope.ServiceProvider.GetRequiredService<Protos.MailService.MailServiceClient>();
        var rsaKeyService = scope.ServiceProvider.GetRequiredService<RsaKeyService>();
        var jwtOtions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<JwtOptions>>().Value;

        if (DateTime.Now.AddMinutes(1) > _expires || string.IsNullOrEmpty(_jwtToken))
        {
            var admin = await userManager.FindByNameAsync(UserConsts.AdministratorUserName);
            if (admin is null)
            {
                return (false, "系统管理员不存在，无法发送系统邮件");
            }
            var roles = await userManager.GetRolesAsync(admin);

            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new RsaSecurityKey(rsaKeyService.PrivateKeyParam);
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            var expires = DateTime.Now.Add(TimeSpan.FromMinutes(jwtOtions.AccessExpirationMinutes));
            var token = new JwtSecurityToken(
                issuer: jwtOtions.Issuer,
                audience: jwtOtions.Issuer,
                claims: claims,
                expires: expires,
                notBefore: DateTime.Now,
                signingCredentials: creds);
            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);

            _expires = expires;
            _jwtToken = writeToken;
        }

        var header = new Grpc.Core.Metadata
        {
            { "Authorization", $"Bearer {_jwtToken}" }
        };

        try
        {
            var result = mailService.SendEmail(new SendEmailRequest
            {
                FromName = "SmartHome-授权中心",
                ToName = toName,
                ToEmail = toEmail,
                Subject = subject,
                Body = body
            }, header);

            return (result.Success, result.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

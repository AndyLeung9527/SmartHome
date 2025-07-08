namespace Identity.Api.Controllers.V1;

/// <summary>
/// 账户管理
/// </summary>
[ApiVersion(1)]
[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]/[action]")]
public class AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RedisService redisService, RsaKeyService rsaKeyService, IOptionsSnapshot<IdentityOptions> identityOptions, IOptionsSnapshot<JwtOptions> jwtOtions, IdGenerator idGenerator, Services.MailService mailService) : ControllerBase
{
    /// <summary>
    /// 注册
    /// </summary>
    [HttpPost]
    public async Task<Results<Ok, BadRequest<string>>> Register([FromBody] RegisterDto dto)
    {
        var user = new User(idGenerator.CreateId(), dto.Name, dto.Email, dto.DateOfBirth);
        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return TypedResults.BadRequest(errors);
        }
        var addRoleResult = await userManager.AddToRoleAsync(user, RoleConsts.GuestRoleName);
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            return TypedResults.BadRequest(errors);
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var url = $"{dto.CallbackUrl}?{dto.EmailQueryParam}={user.Email}&{dto.TokenQueryParam}={HttpUtility.UrlEncode(token)}";
        var body = $"""
            <h1>欢迎注册</h1>
            <p>{user.UserName}：请点击下面的链接确认您的邮箱地址：</p>
            <a href='{url}'>{url}</a>
        """;

        var result = await mailService.SendEmailAsync(user.UserName!, user.Email!, "欢迎注册", body);
        if (!result.Item1)
        {
            return TypedResults.BadRequest(result.Item2);
        }

        return TypedResults.Ok();
    }

    /// <summary>
    /// 确认邮箱
    /// </summary>
    [HttpPut]
    public async Task<Results<Ok, BadRequest<string>>> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return TypedResults.BadRequest("用户不存在");
        }

        var result = await userManager.ConfirmEmailAsync(user, dto.Token);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(string.Join(", ", result.Errors.Select(o => o.Description)));
        }

        return TypedResults.Ok();
    }

    /// <summary>
    /// 获取用户名、访问令牌和刷新令牌
    /// </summary>
    [HttpPut]
    public async Task<Results<Ok<JwtTokenResponse>, BadRequest<string>>> JwtToken([FromBody] JwtTokenDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.NameOrEmail);
        if (user is null)
        {
            user = await userManager.FindByNameAsync(dto.NameOrEmail);
        }
        if (user is null)
        {
            return TypedResults.BadRequest("用户不存在");
        }
        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, true);
        if (result.Succeeded)
        {
            if (!user.EmailConfirmed)
            {
                return TypedResults.BadRequest($"请先确认邮箱：{user.Email}");
            }

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = GenJwtToken(user, roles, jwtOtions.Value.Issuer, dto.Audience, TimeSpan.FromMinutes(jwtOtions.Value.AccessExpirationMinutes));
            var refreshToken = GenJwtToken(user, roles, jwtOtions.Value.Issuer, dto.Audience, TimeSpan.FromMinutes(jwtOtions.Value.RefreshExpirationMinutes));
            await redisService.DefaultDatabase.StringSetAsync(RedisKeys.Wrap(user.Email!), refreshToken, TimeSpan.FromMinutes(jwtOtions.Value.RefreshExpirationMinutes));
            return TypedResults.Ok(new JwtTokenResponse(user.UserName!, accessToken, refreshToken));
        }
        if (user.LockoutEnabled && result.IsLockedOut)
        {
            var remaining = user.LockoutEnd - DateTimeOffset.Now;
            return TypedResults.BadRequest($"账户由于多次登录失败已锁定，请等待{remaining?.Add(TimeSpan.FromMinutes(1)).TotalMinutes ?? 1}分钟后重试");
        }
        if (user.LockoutEnabled && identityOptions.Value.Lockout.MaxFailedAccessAttempts > 0 && user.AccessFailedCount >= identityOptions.Value.Lockout.MaxFailedAccessAttempts)
        {
            await redisService.DefaultDatabase.KeyDeleteAsync(RedisKeys.Wrap(user.Email!));
        }
        return TypedResults.BadRequest("无效的用户名或密码");
    }

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    [HttpGet]
    public async Task<Results<Ok<RefreshAccessTokenResponse>, BadRequest<string>>> RefreshAccessToken([FromQuery] string refreshToken)
    {
        JwtPayload? payload = default;
        try
        {
            payload = new JwtSecurityTokenHandler().ReadJwtToken(refreshToken).Payload;
        }
        catch
        {
            return TypedResults.BadRequest($"刷新令牌非法");
        }
        if (payload is null)
        {
            return TypedResults.BadRequest("刷新令牌无效");
        }
        if (DateTimeOffset.Now > DateTimeOffset.FromUnixTimeSeconds(payload.Expiration ?? default))
        {
            return TypedResults.BadRequest("刷新令牌已过期");
        }
        var email = payload.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return TypedResults.BadRequest("用户不存在");
        }
        var redisRefreshToken = await redisService.DefaultDatabase.StringGetAsync(RedisKeys.Wrap(email));
        if (redisRefreshToken.IsNullOrEmpty)
        {
            return TypedResults.BadRequest("刷新令牌已失效");
        }
        if (redisRefreshToken != refreshToken)
        {
            return TypedResults.BadRequest("刷新令牌不匹配");
        }

        var roles = await userManager.GetRolesAsync(user);
        var audience = payload.Claims.FirstOrDefault(o => o.Type == JwtRegisteredClaimNames.Aud)?.Value ?? string.Empty;
        var accessToken = GenJwtToken(user, roles, jwtOtions.Value.Issuer, audience, TimeSpan.FromMinutes(jwtOtions.Value.AccessExpirationMinutes));

        return TypedResults.Ok(new RefreshAccessTokenResponse(accessToken));
    }

    /// <summary>
    /// 忘记密码
    /// </summary>
    /// <returns>用户邮箱</returns>
    [HttpPut]
    public async Task<Results<Ok<string>, BadRequest<string>>> ForgotPassword([FromBody] ForgotPasswordTokenDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.NameOrEmail);
        if (user is null)
        {
            user = await userManager.FindByNameAsync(dto.NameOrEmail);
        }
        if (user is null)
        {
            return TypedResults.BadRequest("用户不存在");
        }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var url = $"{dto.CallbackUrl}?{dto.EmailQueryParam}={user.Email}&{dto.TokenQueryParam}={HttpUtility.UrlEncode(token)}";
        var body = $"""
            <h1>重置密码</h1>
            <p>{user.UserName}：请点击下面的链接重置您的密码：</p>
            <a href='{url}'>{url}</a>
        """;

        var result = await mailService.SendEmailAsync(user.UserName!, user.Email!, "重置密码", body);
        if (!result.Item1)
        {
            return TypedResults.BadRequest(result.Item2);
        }

        return TypedResults.Ok(user.Email);
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    [HttpPut]
    public async Task<Results<Ok, BadRequest<string>>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return TypedResults.BadRequest("用户不存在");
        }

        var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(string.Join(", ", result.Errors.Select(o => o.Description)));
        }
        return TypedResults.Ok();
    }

    private string GenJwtToken(User user, IEnumerable<string> roles, string issuer, string audience, TimeSpan expire)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth.ToString())
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new RsaSecurityKey(rsaKeyService.PrivateKeyParam);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.Add(expire),
            notBefore: DateTime.Now,
            signingCredentials: creds);
        var writeToken = new JwtSecurityTokenHandler().WriteToken(token);
        return writeToken;
    }
}

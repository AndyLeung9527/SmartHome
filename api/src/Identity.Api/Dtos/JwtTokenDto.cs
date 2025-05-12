namespace Identity.Api.Dtos;

public class JwtTokenDto
{
    /// <summary>
    /// 请求方
    /// </summary>
    [Required(ErrorMessage = "Audience必填")]
    [StringLength(100, ErrorMessage = "Audience长度不能超过100个字符")]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    [Required(ErrorMessage = "用户名或邮箱必填")]
    [StringLength(100, ErrorMessage = "用户名或邮箱长度不能超过100个字符")]
    public string NameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码必填")]
    [StringLength(100, ErrorMessage = "密码不能超过100个字符")]
    public string Password { get; set; } = string.Empty;
}

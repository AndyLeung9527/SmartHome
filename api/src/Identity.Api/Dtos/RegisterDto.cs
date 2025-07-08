namespace Identity.Api.Dtos;

/// <summary>
/// 注册响应对象
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名必填")]
    [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码必填")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 确认密码
    /// </summary>
    [Required(ErrorMessage = "确认密码必填")]
    [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required(ErrorMessage = "邮箱必填")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 出生日期
    /// </summary>
    [Required(ErrorMessage = "出生日期必填")]
    public DateTimeOffset DateOfBirth { get; set; }

    /// <summary>
    /// 回调地址
    /// </summary>
    [Required(ErrorMessage = "CallbackUrl必填")]
    [StringLength(500, ErrorMessage = "CallbackUrl不能超过500个字符")]
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱参数名（Query参数）
    /// </summary>
    [Required(ErrorMessage = "EmailQueryParam必填")]
    [StringLength(100, ErrorMessage = "EmailQueryParam不能超过100个字符")]
    public string EmailQueryParam { get; set; } = string.Empty;

    /// <summary>
    /// token参数名（Query参数）
    /// </summary>
    [Required(ErrorMessage = "TokenQueryParam必填")]
    [StringLength(100, ErrorMessage = "TokenQueryParam不能超过100个字符")]
    public string TokenQueryParam { get; set; } = string.Empty;
}

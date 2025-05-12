namespace Identity.Api.Dtos;

public class ForgotPasswordTokenDto
{
    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    [Required(ErrorMessage = "用户名或邮箱必填")]
    [StringLength(100, ErrorMessage = "用户名或邮箱长度不能超过100个字符")]
    public string NameOrEmail { get; set; } = string.Empty;

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

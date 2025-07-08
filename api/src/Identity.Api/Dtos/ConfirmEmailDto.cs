namespace Identity.Api.Dtos;

public class ConfirmEmailDto
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [Required(ErrorMessage = "邮箱必填")]
    [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 确认邮箱令牌
    /// </summary>
    [Required(ErrorMessage = "确认邮箱令牌必填")]
    public string Token { get; set; } = string.Empty;
}

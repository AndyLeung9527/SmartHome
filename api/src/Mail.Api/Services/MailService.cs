namespace Mail.Api.Services;

[Authorize(Roles = RoleConsts.AdministratorRoleName)]
public class MailService : Protos.MailService.MailServiceBase
{
    private readonly EmailOption _emailOption;

    public MailService(IOptions<EmailOption> emailsOption)
    {
        _emailOption = emailsOption.Value;
    }

    public override async Task<SendEmailResult> SendEmail(SendEmailRequest request, ServerCallContext context)
    {
        var items = _emailOption.Items.Where(o => o.Addresses.Any(o => o.Enable)).ToArray();
        if (items.Length <= 0)
        {
            return new SendEmailResult
            {
                Success = false,
                Message = "没有配置可用的系统邮箱，请联系管理员"
            };
        }
        // 随机选择一个可用的邮箱集
        var item = Random.Shared.GetItems(items, 1).First();
        // 随机选择一个可用的邮箱地址
        var address = Random.Shared.GetItems(item.Addresses.ToArray(), 1).First();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(request.FromName, address.Email));
        message.To.Add(new MailboxAddress(request.ToName, request.ToEmail));
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = request.Body
        };

        bool connected = false;
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(item.Host, item.Port, item.EnableSsl);
            connected = true;
            await client.AuthenticateAsync(address.Email, address.Password);
            await client.SendAsync(message);
            return new SendEmailResult
            {
                Success = true,
                Message = "邮件发送成功"
            };
        }
        catch (Exception ex)
        {
            return new SendEmailResult
            {
                Success = false,
                Message = $"邮件发送失败: {ex.Message}"
            };
        }
        finally
        {
            if (connected)
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}

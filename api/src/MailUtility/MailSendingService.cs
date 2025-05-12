using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace MailUtility;

public class MailSendingService
{
    public static async Task SendMailAsync(string Name, string Mail, string Subject, string Body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Smart home", "andyleung0916@163.com"));
        message.To.Add(new MailboxAddress(Name, Mail));
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = Body
        };
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.163.com", 465, true);
        await client.AuthenticateAsync("andyleung0916@163.com", "TGbwpRhZAKwUBaeb");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

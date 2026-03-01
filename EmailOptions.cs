using Microsoft.Extensions.Options;
public class EmailOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string From { get; set; } = default!;
    public string BusinessInbox { get; set; } = default!;
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
}

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _opt;
    public SmtpEmailService(IOptions<EmailOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        using var client = new System.Net.Mail.SmtpClient(_opt.Host, _opt.Port)
        {
            EnableSsl = true,
            Credentials = new System.Net.NetworkCredential(_opt.Username, _opt.Password)
        };

        var msg = new System.Net.Mail.MailMessage
        {
            From = new System.Net.Mail.MailAddress(_opt.From),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(to);

        await client.SendMailAsync(msg);
    }
}
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

public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public BrevoEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var apiKey = _config["Brevo:ApiKey"];

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        var body = new
        {
            sender = new { name = "RA Nail Art", email = "miray0853@gmail.com" },
            to = new[] { new { email = to } },
            subject = subject,
            htmlContent = htmlBody
        };

        var response = await client.PostAsJsonAsync(
            "https://api.brevo.com/v3/smtp/email",
            body
        );

        response.EnsureSuccessStatusCode();
    }
}
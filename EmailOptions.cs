using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class BrevoOptions
{
    public string ApiKey { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = "RA Nail Art";
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
}

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly BrevoOptions _opt;

    public BrevoEmailService(HttpClient http, IOptions<BrevoOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new Exception("Brevo ApiKey boş. Render ENV: Brevo__ApiKey kontrol et (API Key olmalı).");

        var url = "https://api.brevo.com/v3/smtp/email";

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.Add("api-key", _opt.ApiKey);

        var payload = new
        {
            sender = new { name = _opt.FromName, email = _opt.FromEmail },
            to = new[] { new { email = to } },
            subject = subject,
            htmlContent = htmlBody
        };

        var json = JsonSerializer.Serialize(payload);
        var res = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new Exception($"Brevo API hata: {(int)res.StatusCode} - {body}");
        }
    }
}
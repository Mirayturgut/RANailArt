using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public AppointmentsController(AppDbContext db, IEmailService email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    // TEST
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("pong");

    // DB bağlantı testi
    [HttpGet("dbping")]
    public async Task<IActionResult> DbPing()
    {
        try
        {
            var can = await _db.Database.CanConnectAsync();
            return Ok(new { canConnect = can });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var start = dto.StartAt;
        var end = dto.StartAt.AddMinutes(dto.DurationMinutes);

        var conflict = await _db.Appointments.AnyAsync(a =>
            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved) &&
            a.StartAt < end &&
            a.StartAt.AddMinutes(a.DurationMinutes) > start
        );

        if (conflict) return Conflict("Bu saat dolu. Lütfen başka saat seçin.");

        var token = TokenGen.CreateUrlSafeToken();

        var appt = new Appointment
        {
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            ServiceName = dto.ServiceName,
            Note = dto.Note,
            StartAt = dto.StartAt,
            DurationMinutes = dto.DurationMinutes,
            DecisionToken = token,
            TokenExpiresAt = DateTime.UtcNow.AddHours(24),
            Status = AppointmentStatus.Pending
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        var approveUrl = Url.Action("Decide", "Business", new { token, decision = "approve" }, Request.Scheme);
        var rejectUrl  = Url.Action("Decide", "Business", new { token, decision = "reject" }, Request.Scheme);

        var html = $@"
<h2>Yeni Randevu Talebi</h2>
<p><b>Müşteri:</b> {appt.CustomerName}</p>
<p><b>Email:</b> {appt.CustomerEmail}</p>
<p><b>Telefon:</b> {appt.CustomerPhone}</p>
<p><b>İşlem:</b> {appt.ServiceName}</p>
<p><b>Tarih/Saat:</b> {appt.StartAt:dd.MM.yyyy HH:mm}</p>
<p><b>Not:</b> {System.Net.WebUtility.HtmlEncode(appt.Note ?? "-")}</p>
<hr/>
<p>
  <a style='padding:10px 14px;background:#16a34a;color:white;text-decoration:none;border-radius:8px' href='{approveUrl}'>✅ Onayla</a>
  &nbsp;
  <a style='padding:10px 14px;background:#dc2626;color:white;text-decoration:none;border-radius:8px' href='{rejectUrl}'>❌ Reddet</a>
</p>
<p style='color:#6b7280;font-size:12px'>Link 24 saat geçerlidir.</p>";

        var businessInbox = _config["Email:BusinessInbox"];
        if (string.IsNullOrWhiteSpace(businessInbox))
            return StatusCode(500, "Email:BusinessInbox ayarı yok. Render ENV: Email__BusinessInbox ekle.");

        try
        {
            await _email.SendAsync(businessInbox, "Yeni Randevu Talebi", html);

            await _email.SendAsync(
                appt.CustomerEmail,
                "Randevu talebiniz alındı",
                $"<p>Merhaba {appt.CustomerName},</p>" +
                $"<p>{appt.StartAt:dd.MM.yyyy HH:mm} için <b>{appt.ServiceName}</b> randevu talebiniz alınmıştır. Onaylanınca size mail atacağız.</p>"
            );
        }
        catch (Exception ex)
        {
            // DB’ye kaydetti ama mail patladıysa bunu net görelim:
            return StatusCode(500, "Mail gönderilemedi: " + ex.Message);
        }

        return Ok(new { appt.Id, appt.Status });
    }
}

public class AppointmentCreateDto
{
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string ServiceName { get; set; } = default!;
    public string? Note { get; set; }
}
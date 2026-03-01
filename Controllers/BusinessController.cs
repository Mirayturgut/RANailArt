using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class BusinessController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public BusinessController(AppDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    // GET /Business/Decide?token=...&decision=approve|reject
    [HttpGet]
    public async Task<IActionResult> Decide(string token, string decision)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Content("❌ Token yok.");

        var appt = await _db.Appointments.FirstOrDefaultAsync(x => x.DecisionToken == token);
        if (appt == null)
            return Content("❌ Randevu bulunamadı.");

        if (DateTime.UtcNow > appt.TokenExpiresAt)
            return Content("⏰ Link süresi dolmuş.");

        // Daha önce karar verildiyse tekrar mail atmasın (idempotent)
        if (appt.Status != AppointmentStatus.Pending)
            return Content($"ℹ️ Bu randevu zaten {appt.Status}.");

        decision = (decision ?? "").Trim().ToLowerInvariant();

        if (decision == "approve")
        {
            // Onaylamadan önce tekrar çakışma kontrolü
            var start = appt.StartAt;
            var end = appt.StartAt.AddMinutes(appt.DurationMinutes);

            var conflict = await _db.Appointments.AnyAsync(a =>
                a.Id != appt.Id &&
                a.Status == AppointmentStatus.Approved &&
                a.StartAt < end &&
                a.StartAt.AddMinutes(a.DurationMinutes) > start
            );

            if (conflict)
            {
                appt.Status = AppointmentStatus.Rejected;
                appt.DecidedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await _email.SendAsync(
                    appt.CustomerEmail,
                    "Randevunuz reddedildi ❌",
                    $"<p>Merhaba {appt.CustomerName},</p>" +
                    $"<p>Maalesef <b>{appt.StartAt:dd.MM.yyyy HH:mm}</b> için randevunuz başka bir randevu ile çakıştığı için reddedildi.</p>"
                );

                return Content("❌ Çakışma olduğu için reddedildi ve müşteriye mail gönderildi.");
            }

            appt.Status = AppointmentStatus.Approved;
            appt.DecidedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _email.SendAsync(
                appt.CustomerEmail,
                "Randevunuz onaylandı ✅",
                $"<p>Merhaba {appt.CustomerName},</p>" +
                $"<p>Randevunuz onaylandı.</p>" +
                $"<p><b>{appt.StartAt:dd.MM.yyyy HH:mm}</b> — {appt.ServiceName}</p>"
            );

            return Content("✅ Onaylandı ve müşteriye mail gönderildi.");
        }

        if (decision == "reject")
        {
            appt.Status = AppointmentStatus.Rejected;
            appt.DecidedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _email.SendAsync(
                appt.CustomerEmail,
                "Randevunuz reddedildi ❌",
                $"<p>Merhaba {appt.CustomerName},</p>" +
                $"<p>Maalesef <b>{appt.StartAt:dd.MM.yyyy HH:mm}</b> için randevunuz reddedildi.</p>"
            );

            return Content("❌ Reddedildi ve müşteriye mail gönderildi.");
        }

        return Content("❌ Geçersiz decision. approve veya reject olmalı.");
    }
}
public enum AppointmentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public class Appointment
{
    public int Id { get; set; }

    // Müşteri bilgileri
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;

    // Randevu detayları
    public DateTime StartAt { get; set; }         // ör: 2026-03-05 14:00
    public int DurationMinutes { get; set; } = 60;
    public string ServiceName { get; set; } = default!; // manikür, nail art vs
    public string? Note { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    // Maildeki onay/red linki için güvenli token
    public string DecisionToken { get; set; } = default!;
    public DateTime TokenExpiresAt { get; set; }

    // Idempotency / tekrar tıklama kontrolü için
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
}
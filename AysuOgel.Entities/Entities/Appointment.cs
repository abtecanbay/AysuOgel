using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public enum AppointmentStatus
{
    [Display(Name = "Beklemede")] Beklemede = 1,
    [Display(Name = "Onaylandı")] Onaylandi = 2,
    [Display(Name = "İptal edildi")] IptalEdildi = 3
}

public class Appointment
{
    public int Id { get; set; }

    public DateTime SlotStart { get; set; }     // Türkiye saati
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Beklemede;

    public int ContactMessageId { get; set; }
    public ContactMessage ContactMessage { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
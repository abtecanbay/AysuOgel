using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public enum SessionStatus
{
    [Display(Name = "Planlandı")] Planlandi = 1,
    [Display(Name = "Yapıldı")] Yapildi = 2,
    [Display(Name = "İptal edildi")] IptalEdildi = 3
}

public class MemberSession
{
    public int Id { get; set; }

    public int MemberPackageId { get; set; }
    public MemberPackage MemberPackage { get; set; } = null!;

    /// <summary>
    /// Paket içindeki seans sırası (1. seans, 5. seans...). Elle girilmez; iptal edilmemiş
    /// seansların tarih sırasından hesaplanır. İptal edilen seanslarda 0'dır.
    /// </summary>
    public int SessionNumber { get; set; }

    public DateTime ScheduledAt { get; set; }   // Türkiye saati

    public SessionStatus Status { get; set; } = SessionStatus.Planlandi;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

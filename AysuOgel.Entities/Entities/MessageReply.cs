using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public enum ReplyDeliveryStatus
{
    [Display(Name = "Gönderim bekliyor")] Bekliyor = 1,
    [Display(Name = "Gönderildi")] Gonderildi = 2,
    [Display(Name = "Gönderilemedi")] Hata = 3
}

public class MessageReply
{
    public int Id { get; set; }

    public int ContactMessageId { get; set; }
    public ContactMessage ContactMessage { get; set; } = null!;

    [MaxLength(150)]
    public string ToEmail { get; set; } = string.Empty;   // gönderim anındaki alıcı

    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public ReplyDeliveryStatus DeliveryStatus { get; set; } = ReplyDeliveryStatus.Bekliyor;

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public enum ContactSubject
{
    [Display(Name = "Bilgi almak istiyorum")] BilgiAlma = 1,
    [Display(Name = "Randevu talebi")] RandevuTalebi = 2,
    [Display(Name = "Hizmetler hakkında soru")] HizmetSorusu = 3,
    [Display(Name = "Diğer")] Diger = 4
}

public class ContactMessage
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public ContactSubject Subject { get; set; }

    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RequestedAppointmentAt { get; set; }
}
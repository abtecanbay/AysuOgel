using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class AvailabilityFormViewModel
{
    public int Id { get; set; }

    // Düzenleme ekranında tek gün kullanılır.
    [Display(Name = "Tarih")]
    public DateOnly? Date { get; set; }

    // Yeni müsaitlik eklerken tarih aralığı kullanılır.
    [Display(Name = "Başlangıç tarihi")]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "Bitiş tarihi")]
    public DateOnly? EndDate { get; set; }

    [Display(Name = "Başlangıç saati")]
    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeOnly? StartTime { get; set; } = new TimeOnly(9, 0);

    [Display(Name = "Bitiş saati (son randevunun bittiği saat)")]
    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeOnly? EndTime { get; set; } = new TimeOnly(18, 0);

    [Display(Name = "Randevu süresi (dk)")]
    [Required(ErrorMessage = "Randevu süresi zorunludur.")]
    [Range(10, 240, ErrorMessage = "10 ile 240 dakika arasında olmalıdır.")]
    public int? SlotMinutes { get; set; } = 30;

    [Display(Name = "Yalnızca hafta içi (Pazartesi-Cuma)")]
    public bool WeekdaysOnly { get; set; } = true;
}
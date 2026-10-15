using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class AvailabilityFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Tarih")]
    [Required(ErrorMessage = "Tarih zorunludur.")]
    public DateOnly? Date { get; set; }

    [Display(Name = "Başlangıç saati")]
    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeOnly? StartTime { get; set; }

    [Display(Name = "Bitiş saati (son randevunun bittiği saat)")]
    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeOnly? EndTime { get; set; }

    [Display(Name = "Randevu süresi (dk)")]
    [Required(ErrorMessage = "Randevu süresi zorunludur.")]
    [Range(10, 240, ErrorMessage = "10 ile 240 dakika arasında olmalıdır.")]
    public int? SlotMinutes { get; set; } = 30;
}
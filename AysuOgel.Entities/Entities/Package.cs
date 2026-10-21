using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public class Package
{
    public int Id { get; set; }

    [Display(Name = "Paket adı")]
    [Required(ErrorMessage = "Paket adı zorunludur.")]
    [MaxLength(150, ErrorMessage = "Paket adı en fazla 150 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string? Description { get; set; }

    [Display(Name = "Seans sayısı")]
    [Range(1, 500, ErrorMessage = "Seans sayısı 1 ile 500 arasında olmalıdır.")]
    public int SessionCount { get; set; } = 1;

    [Display(Name = "Fiyat (₺)")]
    [Range(0, 10_000_000, ErrorMessage = "Geçerli bir fiyat girin.")]
    public decimal Price { get; set; }

    /// <summary>Süreli paketlerde başlangıçtan itibaren geçerlilik süresi; boşsa süresiz.</summary>
    [Display(Name = "Geçerlilik süresi (gün)")]
    [Range(1, 3650, ErrorMessage = "Geçerlilik süresi 1 ile 3650 gün arasında olmalıdır.")]
    public int? ValidityDays { get; set; }

    [Display(Name = "Satışta")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Gösterim sırası")]
    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

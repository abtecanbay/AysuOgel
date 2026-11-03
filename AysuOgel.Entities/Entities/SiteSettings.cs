using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

/// <summary>Sitenin genel ayarları. Bu tabloda tek kayıt tutulur; Id daima 1'dir.</summary>
public class SiteSettings
{
    public int Id { get; set; } = 1;

    [Display(Name = "Site başlığı")]
    [Required(ErrorMessage = "Site başlığı zorunludur.")]
    [MaxLength(150, ErrorMessage = "Site başlığı en fazla 150 karakter olabilir.")]
    public string SiteTitle { get; set; } = "Aysu Öğel Sağlıklı Yaşam";

    [Display(Name = "Marka adı")]
    [Required(ErrorMessage = "Marka adı zorunludur.")]
    [MaxLength(60, ErrorMessage = "Marka adı en fazla 60 karakter olabilir.")]
    public string BrandName { get; set; } = "AysuOgel";

    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public class CampaignPageSettings
{
    // Bu tabloda tek kayıt tutulur; Id daima 1'dir.
    public int Id { get; set; } = 1;

    [Display(Name = "Banner görseli")]
    [MaxLength(1000, ErrorMessage = "Görsel adresi en fazla 1000 karakter olabilir.")]
    public string? BannerImageUrl { get; set; }

    [Display(Name = "Başlık")]
    [Required(ErrorMessage = "Banner başlığı zorunludur.")]
    [MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string IntroTitle { get; set; } = "Kampanyalarımız";

    [Display(Name = "Açıklama")]
    [Required(ErrorMessage = "Banner açıklaması zorunludur.")]
    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string IntroText { get; set; } = "Size özel fırsatları ve güncel kampanyalarımızı keşfedin.";

    public DateTime? UpdatedAt { get; set; }
}
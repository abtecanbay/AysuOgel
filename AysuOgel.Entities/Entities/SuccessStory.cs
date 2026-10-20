using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public class SuccessStory
{
    public int Id { get; set; }

    [Display(Name = "Öncesi fotoğrafı")]
    [MaxLength(1000)]
    public string? BeforeImageUrl { get; set; }

    [Display(Name = "Sonrası fotoğrafı")]
    [MaxLength(1000)]
    public string? AfterImageUrl { get; set; }

    [Display(Name = "Süreç açıklaması")]
    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [MaxLength(600, ErrorMessage = "En fazla 600 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Yayında")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
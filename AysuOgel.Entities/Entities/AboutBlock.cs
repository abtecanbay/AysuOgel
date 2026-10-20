using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

// Değerler Business.Models.BlockLayout ile aynı sırada tutulmalıdır.
public enum AboutBlockLayout
{
    [Display(Name = "Yalnızca metin")]
    TextOnly = 0,

    [Display(Name = "Görsel solda")]
    ImageLeft = 1,

    [Display(Name = "Görsel sağda")]
    ImageRight = 2,

    [Display(Name = "Galeri")]
    Gallery = 3
}

public class AboutBlock
{
    public int Id { get; set; }

    [Display(Name = "Bağlantı adı (slug)")]
    [MaxLength(80, ErrorMessage = "Slug en fazla 80 karakter olabilir.")]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Yalnızca küçük harf, rakam ve tire kullanılabilir. Örn: mekanimiz")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    [Required(ErrorMessage = "Başlık zorunludur.")]
    [MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Metin")]
    [MaxLength(3000, ErrorMessage = "Metin en fazla 3000 karakter olabilir.")]
    public string? Text { get; set; }

    [Display(Name = "Yerleşim")]
    public AboutBlockLayout Layout { get; set; } = AboutBlockLayout.TextOnly;

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Yayında")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<AboutBlockImage> Images { get; set; } = new();
}

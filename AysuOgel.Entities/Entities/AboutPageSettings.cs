using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public class AboutPageSettings
{
    // Bu tabloda tek kayıt tutulur; Id daima 1'dir.
    public int Id { get; set; } = 1;

    [Display(Name = "Sayfa başlığı")]
    [Required(ErrorMessage = "Sayfa başlığı zorunludur.")]
    [MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = "Hakkımızda";

    [Display(Name = "Giriş metni")]
    [MaxLength(1000, ErrorMessage = "Giriş metni en fazla 1000 karakter olabilir.")]
    public string? Intro { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

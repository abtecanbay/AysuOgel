using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public class Campaign : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Kampanya başlığı")]
    [Required(ErrorMessage = "Kampanya başlığı zorunludur.")]
    [MaxLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Kısa açıklama")]
    [MaxLength(300, ErrorMessage = "Kısa açıklama en fazla 300 karakter olabilir.")]
    public string? ShortDescription { get; set; }

    [Display(Name = "Detaylı açıklama")]
    [Required(ErrorMessage = "Kampanya açıklaması zorunludur.")]
    [MaxLength(3000, ErrorMessage = "Açıklama en fazla 3000 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Kampanya görseli")]
    [Required(ErrorMessage = "Kampanya görseli zorunludur.")]
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Başlangıç tarihi")]
    public DateOnly StartDate { get; set; }

    [Display(Name = "Bitiş tarihi")]
    public DateOnly EndDate { get; set; }

    [Display(Name = "Yayında")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Gösterim sırası")]
    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate == default)
            yield return new ValidationResult("Başlangıç tarihi zorunludur.", new[] { nameof(StartDate) });

        if (EndDate == default)
            yield return new ValidationResult("Bitiş tarihi zorunludur.", new[] { nameof(EndDate) });

        if (StartDate != default && EndDate != default && EndDate < StartDate)
            yield return new ValidationResult("Bitiş tarihi başlangıç tarihinden önce olamaz.", new[] { nameof(EndDate) });
    }
}
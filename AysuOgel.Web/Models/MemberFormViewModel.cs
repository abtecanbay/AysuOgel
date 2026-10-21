using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

/// <summary>Üye ekleme / düzenleme formu. Paket alanları yalnızca yeni kayıtta kullanılır.</summary>
public class MemberFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Ad")]
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MaxLength(80, ErrorMessage = "En fazla 80 karakter olabilir.")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Soyad")]
    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MaxLength(80, ErrorMessage = "En fazla 80 karakter olabilir.")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "E-posta")]
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [MaxLength(150, ErrorMessage = "En fazla 150 karakter olabilir.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefon")]
    [Required(ErrorMessage = "Telefon zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
    [MaxLength(30, ErrorMessage = "En fazla 30 karakter olabilir.")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Şifre")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string? Password { get; set; }

    [Display(Name = "Şifre (tekrar)")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Satın aldığı paket")]
    public int? PackageId { get; set; }

    [Display(Name = "Başlama tarihi")]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "Bitiş tarihi")]
    public DateOnly? EndDate { get; set; }

    public bool IsNew => Id == 0;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsNew) yield break;

        if (string.IsNullOrEmpty(Password))
            yield return new ValidationResult("Şifre zorunludur.", new[] { nameof(Password) });

        if (PackageId is null)
            yield return new ValidationResult("Paket seçimi zorunludur.", new[] { nameof(PackageId) });

        if (StartDate is null)
            yield return new ValidationResult("Başlama tarihi zorunludur.", new[] { nameof(StartDate) });

        if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
            yield return new ValidationResult("Bitiş tarihi başlama tarihinden önce olamaz.", new[] { nameof(EndDate) });
    }
}

using System.ComponentModel.DataAnnotations;
using AysuOgel.Entities.Entities;
using AysuOgel.Web.Validation;

namespace AysuOgel.Web.Models;

public class ContactFormViewModel
{
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
    [RegularExpression(@"^[0-9+\s()\-]{10,20}$", ErrorMessage = "Geçerli bir telefon numarası girin.")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Konu")]
    [Required(ErrorMessage = "Lütfen bir konu seçin.")]
    public ContactSubject? Subject { get; set; }

    [Display(Name = "Mesajınız")]
    [Required(ErrorMessage = "Mesaj zorunludur.")]
    [MaxLength(2000, ErrorMessage = "En fazla 2000 karakter olabilir.")]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Onay")]
    [MustBeTrue(ErrorMessage = "Devam etmek için onay vermelisiniz.")]
    public bool KvkkAccepted { get; set; }
    // Bot tuzağı: gerçek kullanıcılar görmez, botlar doldurur.
    public string? Website { get; set; }
    public string? AppointmentSlot { get; set; }
}
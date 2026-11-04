using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class MemberLoginViewModel
{
    [Display(Name = "E-posta veya telefon")]
    [Required(ErrorMessage = "E-posta veya telefon numarası zorunludur.")]
    [StringLength(254, ErrorMessage = "En fazla {1} karakter girilebilir.")]
    public string Login { get; set; } = string.Empty;

    [Display(Name = "Şifre")]
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

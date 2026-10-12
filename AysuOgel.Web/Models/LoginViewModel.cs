using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class LoginViewModel
{
    [Display(Name = "Kullanıcı Adı")]
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Şifre")]
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
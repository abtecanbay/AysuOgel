using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class AdminUserFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Kullanıcı adı")]
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter olmalıdır.")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Yalnızca harf, rakam, nokta, tire ve alt çizgi kullanılabilir.")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Ad soyad")]
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [MaxLength(120, ErrorMessage = "Ad soyad en fazla 120 karakter olabilir.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "E-posta")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [MaxLength(150, ErrorMessage = "E-posta en fazla 150 karakter olabilir.")]
    public string? Email { get; set; }

    [Display(Name = "Şifre")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    public string? Password { get; set; }

    [Display(Name = "Şifre (tekrar)")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    /// <summary>Formda işaretlenen, yöneticinin erişebileceği sayfaların kimlikleri.</summary>
    public List<int> PageIds { get; set; } = [];

    /// <summary>Yetki kartında listelenen tüm sayfalar (formdan bağlanmaz).</summary>
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public List<AysuOgel.Entities.Entities.AdminPage> AvailablePages { get; set; } = [];

    public bool IsNew => Id == 0;
}

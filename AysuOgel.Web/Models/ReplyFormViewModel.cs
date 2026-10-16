using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class ReplyFormViewModel
{
    [Display(Name = "Konu")]
    [Required(ErrorMessage = "Konu zorunludur.")]
    [MaxLength(200, ErrorMessage = "En fazla 200 karakter olabilir.")]
    public string Subject { get; set; } = string.Empty;

    [Display(Name = "Detaylar")]
    [Required(ErrorMessage = "Yanıt metni zorunludur.")]
    [MaxLength(4000, ErrorMessage = "En fazla 4000 karakter olabilir.")]
    public string Body { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Web.Models;

public class MemberPackageFormViewModel
{
    public int Id { get; set; }
    public int MemberId { get; set; }

    [Display(Name = "Paket")]
    public int? PackageId { get; set; }

    [Display(Name = "Başlama tarihi")]
    [Required(ErrorMessage = "Başlama tarihi zorunludur.")]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "Bitiş tarihi")]
    public DateOnly? EndDate { get; set; }
}

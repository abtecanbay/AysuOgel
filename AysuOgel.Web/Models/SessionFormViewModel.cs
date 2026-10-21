using System.ComponentModel.DataAnnotations;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class SessionFormViewModel
{
    public int Id { get; set; }
    public int MemberPackageId { get; set; }

    [Display(Name = "Tarih ve saat")]
    [Required(ErrorMessage = "Seans tarihi ve saati zorunludur.")]
    public DateTime? ScheduledAt { get; set; }

    [Display(Name = "Durum")]
    public SessionStatus Status { get; set; } = SessionStatus.Planlandi;

    [Display(Name = "Not")]
    [MaxLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
    public string? Note { get; set; }

    /// <summary>Yönetici çakışma uyarısını gördü ve yine de kaydetmeyi onayladı.</summary>
    public bool ConfirmConflicts { get; set; }
}

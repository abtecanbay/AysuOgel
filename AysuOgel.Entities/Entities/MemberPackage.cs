using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public enum MemberPackageStatus
{
    [Display(Name = "Devam ediyor")] Aktif = 1,
    [Display(Name = "Tamamlandı")] Tamamlandi = 2
}

/// <summary>Üyenin satın aldığı paket. Seans sayısı satın alma anındaki değerle saklanır.</summary>
public class MemberPackage
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int PackageId { get; set; }
    public Package Package { get; set; } = null!;

    public int SessionCount { get; set; }

    public DateOnly StartDate { get; set; }

    /// <summary>Süreli pakette planlanan bitiş; son seans yapıldığında o seansın tarihiyle güncellenir.</summary>
    public DateOnly? EndDate { get; set; }

    public MemberPackageStatus Status { get; set; } = MemberPackageStatus.Aktif;

    public List<MemberSession> Sessions { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

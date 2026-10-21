using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Models;

public class MemberFilter : PagedFilter
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? PackageId { get; set; }
    public MemberPackageStatus? Status { get; set; }

    /// <summary>Paket başlangıç tarihi aralığı.</summary>
    public DateOnly? StartFrom { get; set; }
    public DateOnly? StartTo { get; set; }

    /// <summary>"asc" veya "desc". Boşsa yeniden eskiye (kayıt tarihi).</summary>
    public string? Sort { get; set; }

    public bool Descending => Sort?.ToLowerInvariant() switch
    {
        "asc" => false,
        _ => true
    };
}

/// <summary>Üye listesinde bir satır; en son başlayan paket gösterilir.</summary>
public class MemberListItem
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int PackageCount { get; set; }
    public string? PackageName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public MemberPackageStatus? Status { get; set; }
    public int DoneSessions { get; set; }
    public int TotalSessions { get; set; }
}

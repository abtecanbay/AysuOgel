namespace AysuOgel.Business.Models;

public class ActivityLogFilter : PagedFilter
{
    /// <summary>"system" = ana yönetici, sayı = AdminUserId, boş = herkes. Yalnızca ana yönetici kullanabilir.</summary>
    public string? Admin { get; set; }

    public string? Category { get; set; }
    public string? Search { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    /// <summary>true: yalnızca başarısız işlemler.</summary>
    public bool FailedOnly { get; set; }

    /// <summary>"asc" veya "desc". Boşsa yeniden eskiye.</summary>
    public string? Sort { get; set; }

    public bool Descending => Sort?.ToLowerInvariant() != "asc";
}

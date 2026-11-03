using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class ActivityLogViewModel
{
    public PagedResult<AdminActivityLog> Logs { get; set; } = new([], 1, PagedFilter.DefaultPageSize, 0);
    public ActivityLogFilter Filter { get; set; } = new();

    /// <summary>Ana (sistem) yönetici tüm yöneticilerin kayıtlarını görür; diğerleri yalnızca kendilerininkini.</summary>
    public bool IsSystemAdmin { get; set; }

    /// <summary>Yönetici filtresi için (yalnızca ana yöneticide dolu).</summary>
    public List<AdminUser> Admins { get; set; } = [];

    public string? SystemAdminUsername { get; set; }
    public List<string> Categories { get; set; } = [];

    public bool HasFilter =>
        !string.IsNullOrWhiteSpace(Filter.Admin) ||
        !string.IsNullOrWhiteSpace(Filter.Category) ||
        !string.IsNullOrWhiteSpace(Filter.Search) ||
        Filter.From.HasValue || Filter.To.HasValue || Filter.FailedOnly;
}

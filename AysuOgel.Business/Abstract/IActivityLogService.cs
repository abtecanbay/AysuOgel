using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

/// <summary>Yönetici işlem kayıtları (log).</summary>
public interface IActivityLogService
{
    /// <summary>Kaydı yazar. Hata olursa asıl isteği bozmamak için hatayı yutar ve uygulama loguna yazar.</summary>
    Task WriteAsync(AdminActivityLog entry);

    /// <summary>
    /// Filtrelenmiş kayıtlar. <paramref name="restrictToAdminUserId"/> verilirse (ana yönetici değilse)
    /// filtredeki yönetici seçimi yok sayılır ve yalnızca o yöneticinin kayıtları döner.
    /// </summary>
    Task<PagedResult<AdminActivityLog>> GetPagedAsync(ActivityLogFilter filter, int? restrictToAdminUserId);

    /// <summary>Kayıtlarda geçen sayfa (kategori) adları.</summary>
    Task<List<string>> GetCategoriesAsync(int? restrictToAdminUserId);
}

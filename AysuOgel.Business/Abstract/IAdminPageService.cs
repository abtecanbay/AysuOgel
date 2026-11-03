using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

/// <summary>Yönetim paneli sayfaları ve yöneticilerin sayfa yetkileri.</summary>
public interface IAdminPageService
{
    /// <summary>Aktif tüm sayfalar, sıralı (önbellekli).</summary>
    Task<List<AdminPage>> GetAllCachedAsync();

    /// <summary>
    /// Kullanıcının erişebildiği aktif sayfalar. <paramref name="adminUserId"/> null ise ana (sistem)
    /// yöneticidir ve tüm sayfalara erişir.
    /// </summary>
    Task<List<AdminPage>> GetAccessibleAsync(int? adminUserId);

    /// <summary>
    /// Kullanıcı verilen controller'a erişebilir mi? Hiçbir sayfaya bağlı olmayan controller'lar
    /// yalnızca ana yöneticiye açıktır.
    /// </summary>
    Task<bool> CanAccessAsync(int? adminUserId, string controller);

    /// <summary>Yöneticiye atanmış sayfa kimlikleri.</summary>
    Task<List<int>> GetPageIdsAsync(int adminUserId);

    /// <summary>Yöneticinin sayfa yetkilerini verilen listeyle değiştirir.</summary>
    Task SetPagesAsync(int adminUserId, IEnumerable<int> pageIds);

    /// <summary>Yöneticinin önbellekteki yetkilerini temizler (ör. silindiğinde).</summary>
    void InvalidateUser(int adminUserId);
}

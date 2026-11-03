using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface ISiteSettingsService
{
    /// <summary>Layout'larda kullanılır; kısa süreli önbellekten okunur.</summary>
    Task<SiteSettings> GetCachedAsync();

    Task<SiteSettings> GetAsync();
    Task UpdateAsync(SiteSettings settings);
}

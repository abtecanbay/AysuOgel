using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AysuOgel.Business.Concrete;

public class SiteSettingsService : ISiteSettingsService
{
    private const string CacheKey = "site-settings";

    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public SiteSettingsService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<SiteSettings> GetCachedAsync() =>
        await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1)
                   ?? new SiteSettings();
        }) ?? new SiteSettings();

    public async Task<SiteSettings> GetAsync()
    {
        var settings = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1);
        return settings ?? new SiteSettings();
    }

    public async Task UpdateAsync(SiteSettings settings)
    {
        var entity = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (entity is null)
        {
            entity = new SiteSettings { Id = 1 };
            _db.SiteSettings.Add(entity);
        }

        entity.SiteTitle = settings.SiteTitle.Trim();
        entity.BrandName = settings.BrandName.Trim();
        entity.LogoUrl = string.IsNullOrWhiteSpace(settings.LogoUrl) ? null : settings.LogoUrl.Trim();
        entity.FaviconUrl = string.IsNullOrWhiteSpace(settings.FaviconUrl) ? null : settings.FaviconUrl.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _cache.Remove(CacheKey);
    }
}

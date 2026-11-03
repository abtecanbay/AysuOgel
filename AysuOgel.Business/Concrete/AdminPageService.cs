using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AysuOgel.Business.Concrete;

public class AdminPageService : IAdminPageService
{
    private const string PagesCacheKey = "admin-pages";
    private static string UserCacheKey(int id) => $"admin-user-pages:{id}";

    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public AdminPageService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<AdminPage>> GetAllCachedAsync() =>
        await _cache.GetOrCreateAsync(PagesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _db.AdminPages.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Title)
                .ToListAsync();
        }) ?? [];

    public async Task<List<AdminPage>> GetAccessibleAsync(int? adminUserId)
    {
        var pages = await GetAllCachedAsync();
        if (adminUserId is null) return pages;

        var allowed = await GetCachedPageIdsAsync(adminUserId.Value);
        return pages.Where(p => allowed.Contains(p.Id)).ToList();
    }

    public async Task<bool> CanAccessAsync(int? adminUserId, string controller)
    {
        if (adminUserId is null) return true;

        var page = (await GetAllCachedAsync()).FirstOrDefault(p => Matches(p, controller));
        if (page is null) return false;

        return (await GetCachedPageIdsAsync(adminUserId.Value)).Contains(page.Id);
    }

    public async Task<AdminPage?> FindByControllerAsync(string controller) =>
        (await GetAllCachedAsync()).FirstOrDefault(p => Matches(p, controller));

    public Task<List<int>> GetPageIdsAsync(int adminUserId) =>
        _db.AdminUserPages.AsNoTracking()
            .Where(x => x.AdminUserId == adminUserId)
            .Select(x => x.AdminPageId)
            .ToListAsync();

    public async Task SetPagesAsync(int adminUserId, IEnumerable<int> pageIds)
    {
        var validIds = await _db.AdminPages
            .Where(p => pageIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var current = await _db.AdminUserPages.Where(x => x.AdminUserId == adminUserId).ToListAsync();

        _db.AdminUserPages.RemoveRange(current.Where(x => !validIds.Contains(x.AdminPageId)));
        foreach (var id in validIds.Where(id => current.All(x => x.AdminPageId != id)))
            _db.AdminUserPages.Add(new AdminUserPage { AdminUserId = adminUserId, AdminPageId = id });

        await _db.SaveChangesAsync();
        InvalidateUser(adminUserId);
    }

    public void InvalidateUser(int adminUserId) => _cache.Remove(UserCacheKey(adminUserId));

    private async Task<HashSet<int>> GetCachedPageIdsAsync(int adminUserId) =>
        await _cache.GetOrCreateAsync(UserCacheKey(adminUserId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return (await GetPageIdsAsync(adminUserId)).ToHashSet();
        }) ?? [];

    private static bool Matches(AdminPage page, string controller) =>
        string.Equals(page.Controller, controller, StringComparison.OrdinalIgnoreCase) ||
        (page.RelatedControllers ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(c => string.Equals(c, controller, StringComparison.OrdinalIgnoreCase));
}

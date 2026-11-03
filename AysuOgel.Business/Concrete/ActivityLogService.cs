using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AysuOgel.Business.Concrete;

public class ActivityLogService : IActivityLogService
{
    // Türkiye saati (UTC+3, yaz saati yok): tarih filtreleri gün sınırlarını buna göre alır.
    private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);

    private readonly AppDbContext _db;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(AppDbContext db, ILogger<ActivityLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task WriteAsync(AdminActivityLog entry)
    {
        try
        {
            entry.Id = 0;
            entry.CreatedAt = DateTime.UtcNow;
            entry.Username = Cut(entry.Username, 50) ?? string.Empty;
            entry.FullName = Cut(entry.FullName, 120) ?? string.Empty;
            entry.Category = Cut(entry.Category, 80) ?? string.Empty;
            entry.Controller = Cut(entry.Controller, 80) ?? string.Empty;
            entry.Action = Cut(entry.Action, 80) ?? string.Empty;
            entry.EntityId = Cut(entry.EntityId, 50);
            entry.Target = Cut(entry.Target, 200);
            entry.Description = Cut(entry.Description, 1000) ?? string.Empty;
            entry.IpAddress = Cut(entry.IpAddress, 45);

            // Asıl işlemden kalan izlenen nesneler varsa onlar yeniden kaydedilmesin; yalnızca log yazılsın.
            _db.ChangeTracker.Clear();
            _db.AdminActivityLogs.Add(entry);
            await _db.SaveChangesAsync();
            _db.Entry(entry).State = EntityState.Detached;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yönetici işlem kaydı yazılamadı: {Controller}/{Action}", entry.Controller, entry.Action);
        }
    }

    public Task<PagedResult<AdminActivityLog>> GetPagedAsync(ActivityLogFilter filter, int? restrictToAdminUserId)
    {
        var query = Scope(restrictToAdminUserId);

        if (restrictToAdminUserId is null && !string.IsNullOrWhiteSpace(filter.Admin))
        {
            if (filter.Admin == "system")
                query = query.Where(x => x.IsSystemAdmin);
            else if (int.TryParse(filter.Admin, out var adminId))
                query = query.Where(x => x.AdminUserId == adminId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(x => x.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(x =>
                x.Description.Contains(s) ||
                (x.Target != null && x.Target.Contains(s)) ||
                x.FullName.Contains(s) ||
                x.Username.Contains(s));
        }

        if (filter.From is { } from)
        {
            var fromUtc = from.ToDateTime(TimeOnly.MinValue) - TurkeyOffset;
            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (filter.To is { } to)
        {
            var toUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue) - TurkeyOffset;
            query = query.Where(x => x.CreatedAt < toUtc);
        }

        if (filter.FailedOnly)
            query = query.Where(x => !x.IsSuccess);

        query = filter.Descending
            ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id);

        return query.ToPagedAsync(filter.Page, filter.PageSize);
    }

    public Task<List<string>> GetCategoriesAsync(int? restrictToAdminUserId) =>
        Scope(restrictToAdminUserId)
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

    /// <summary>Ana yönetici değilse yalnızca kendi kayıtları.</summary>
    private IQueryable<AdminActivityLog> Scope(int? restrictToAdminUserId)
    {
        var query = _db.AdminActivityLogs.AsNoTracking();
        return restrictToAdminUserId is { } id
            ? query.Where(x => x.AdminUserId == id && !x.IsSystemAdmin)
            : query;
    }

    private static string? Cut(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        return value.Length <= max ? value : value[..(max - 1)] + "…";
    }
}

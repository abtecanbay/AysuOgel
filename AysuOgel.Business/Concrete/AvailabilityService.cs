using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class AvailabilityService : IAvailabilityService
{
    private readonly AppDbContext _db;
    public AvailabilityService(AppDbContext db) => _db = db;

    public Task<PagedResult<AvailabilityWindow>> GetPagedAsync(
        DateOnly? startDate, DateOnly? endDate, int page, int pageSize)
    {
        var query = _db.AvailabilityWindows.AsNoTracking();

        if (startDate.HasValue || endDate.HasValue)
        {
            if (startDate.HasValue)
                query = query.Where(w => w.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(w => w.Date <= endDate.Value);
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));
            query = query.Where(w => w.Date >= today);
        }

        return query
            .OrderBy(w => w.Date)
            .ThenBy(w => w.StartTime)
            .ThenBy(w => w.Id)
            .ToPagedAsync(page, pageSize);
    }

    public Task<AvailabilityWindow?> GetByIdAsync(int id) =>
        _db.AvailabilityWindows.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> HasOverlapAsync(DateOnly date, TimeOnly start, TimeOnly end, int? excludeId = null) =>
        _db.AvailabilityWindows.AnyAsync(w =>
            w.Date == date
            && (excludeId == null || w.Id != excludeId)
            && w.StartTime < end && start < w.EndTime);

    public async Task CreateAsync(AvailabilityWindow window)
    {
        window.CreatedAt = DateTime.UtcNow;
        _db.AvailabilityWindows.Add(window);
        await _db.SaveChangesAsync();
    }

    public async Task CreateRangeAsync(IReadOnlyCollection<AvailabilityWindow> windows)
    {
        if (windows.Count == 0) return;

        var createdAt = DateTime.UtcNow;
        foreach (var window in windows)
            window.CreatedAt = createdAt;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.AvailabilityWindows.AddRange(windows);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task UpdateAsync(AvailabilityWindow window)
    {
        var entity = await _db.AvailabilityWindows.FindAsync(window.Id);
        if (entity is null) return;

        entity.Date = window.Date;
        entity.StartTime = window.StartTime;
        entity.EndTime = window.EndTime;
        entity.SlotMinutes = window.SlotMinutes;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.AvailabilityWindows.FindAsync(id);
        if (entity is null) return;
        _db.AvailabilityWindows.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _db;
    public ServiceService(AppDbContext db) => _db = db;

    public Task<List<Service>> GetActiveAsync() =>
        _db.Services.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public Task<List<Service>> GetAllAsync() =>
        _db.Services.AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    public Task<Service?> GetByIdAsync(int id) =>
        _db.Services.FirstOrDefaultAsync(x => x.Id == id);

    public async Task CreateAsync(Service service)
    {
        service.CreatedAt = DateTime.UtcNow;
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        var entity = await _db.Services.FindAsync(service.Id);
        if (entity is null) return;

        entity.Name = service.Name;
        entity.ShortDescription = service.ShortDescription;
        entity.Description = service.Description;
        entity.ImageUrl = service.ImageUrl;
        entity.DisplayOrder = service.DisplayOrder;
        entity.IsActive = service.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Services.FindAsync(id);
        if (entity is null) return;
        _db.Services.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
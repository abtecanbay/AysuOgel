using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class PackageService : IPackageService
{
    private readonly AppDbContext _db;

    public PackageService(AppDbContext db) => _db = db;

    public Task<List<Package>> GetAllAsync() =>
        _db.Packages.AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync();

    public Task<List<Package>> GetActiveAsync() =>
        _db.Packages.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync();

    public Task<Package?> GetByIdAsync(int id) =>
        _db.Packages.FirstOrDefaultAsync(x => x.Id == id);

    public async Task CreateAsync(Package package)
    {
        package.CreatedAt = DateTime.UtcNow;
        _db.Packages.Add(package);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Package package)
    {
        var entity = await _db.Packages.FindAsync(package.Id);
        if (entity is null) return;

        entity.Name = package.Name;
        entity.Description = package.Description;
        entity.SessionCount = package.SessionCount;
        entity.Price = package.Price;
        entity.ValidityDays = package.ValidityDays;
        entity.IsActive = package.IsActive;
        entity.DisplayOrder = package.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Packages.FindAsync(id);
        if (entity is null) return true;

        if (await _db.MemberPackages.AnyAsync(x => x.PackageId == id))
            return false;

        _db.Packages.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}

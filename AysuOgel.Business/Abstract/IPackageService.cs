using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IPackageService
{
    Task<List<Package>> GetAllAsync();
    Task<List<Package>> GetActiveAsync();
    Task<Package?> GetByIdAsync(int id);
    Task CreateAsync(Package package);
    Task UpdateAsync(Package package);

    /// <summary>Üyeye satılmış paket silinmez; false döner.</summary>
    Task<bool> DeleteAsync(int id);
}

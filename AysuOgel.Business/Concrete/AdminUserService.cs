using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class AdminUserService : IAdminUserService
{
    // Kullanıcı bulunamadığında da aynı sürede yanıt vermek için kullanılan sahte özet.
    private static readonly string DummyHash = PasswordHasher.Hash(Guid.NewGuid().ToString());

    private readonly AppDbContext _db;
    public AdminUserService(AppDbContext db) => _db = db;

    public static string Normalize(string username) => username.Trim().ToLowerInvariant();

    public Task<List<AdminUser>> GetAllAsync() =>
        _db.AdminUsers.AsNoTracking()
            .Include(x => x.Pages).ThenInclude(x => x.AdminPage)
            .OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName)
            .ToListAsync();

    public Task<AdminUser?> GetByIdAsync(int id) =>
        _db.AdminUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
    {
        var normalized = Normalize(username);
        return _db.AdminUsers.AnyAsync(x => x.Username == normalized && (excludeId == null || x.Id != excludeId));
    }

    public async Task<AdminUser?> ValidateCredentialsAsync(string username, string password)
    {
        var normalized = Normalize(username);
        var user = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Username == normalized);

        var valid = PasswordHasher.Verify(password, user?.PasswordHash ?? DummyHash);
        if (user is null || !valid || !user.IsActive) return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task CreateAsync(AdminUser user, string password)
    {
        user.Id = 0;
        user.Username = Normalize(user.Username);
        user.FullName = user.FullName.Trim();
        user.Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim().ToLowerInvariant();
        user.PasswordHash = PasswordHasher.Hash(password);
        user.CreatedAt = DateTime.UtcNow;

        _db.AdminUsers.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdminUser user, string? newPassword)
    {
        var entity = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Id == user.Id)
                     ?? throw new InvalidOperationException("Yönetici bulunamadı.");

        entity.Username = Normalize(user.Username);
        entity.FullName = user.FullName.Trim();
        entity.Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim().ToLowerInvariant();
        entity.IsActive = user.IsActive;
        if (!string.IsNullOrEmpty(newPassword))
            entity.PasswordHash = PasswordHasher.Hash(newPassword);
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return;

        _db.AdminUsers.Remove(entity);
        await _db.SaveChangesAsync();
    }
}

using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IAdminUserService
{
    Task<List<AdminUser>> GetAllAsync();
    Task<AdminUser?> GetByIdAsync(int id);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);

    /// <summary>Kullanıcı adı/şifre doğruysa ve hesap aktifse kullanıcıyı döner, son giriş zamanını günceller.</summary>
    Task<AdminUser?> ValidateCredentialsAsync(string username, string password);

    Task CreateAsync(AdminUser user, string password);
    Task UpdateAsync(AdminUser user, string? newPassword);
    Task DeleteAsync(int id);
}

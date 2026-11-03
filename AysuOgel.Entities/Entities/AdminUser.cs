namespace AysuOgel.Entities.Entities;

/// <summary>Yönetim paneline giriş yapabilen kullanıcı. Yalnızca kendisine atanan sayfalara erişebilir.</summary>
public class AdminUser
{
    public int Id { get; set; }

    /// <summary>Küçük harfe çevrilmiş halde saklanır.</summary>
    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }

    /// <summary>PBKDF2 özeti; düz şifre hiçbir zaman saklanmaz.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Erişebildiği yönetim sayfaları.</summary>
    public ICollection<AdminUserPage> Pages { get; set; } = [];
}

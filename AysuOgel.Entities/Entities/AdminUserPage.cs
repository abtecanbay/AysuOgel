namespace AysuOgel.Entities.Entities;

/// <summary>Bir yöneticinin erişebildiği sayfa (çoka-çok ara tablo).</summary>
public class AdminUserPage
{
    public int AdminUserId { get; set; }
    public AdminUser AdminUser { get; set; } = null!;

    public int AdminPageId { get; set; }
    public AdminPage AdminPage { get; set; } = null!;
}

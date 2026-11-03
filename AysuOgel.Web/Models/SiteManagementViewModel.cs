using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class SiteManagementViewModel
{
    public SiteSettings Settings { get; set; } = new();
    public List<AdminUser> Admins { get; set; } = [];

    /// <summary>appsettings / user-secrets içindeki ana yönetici hesabı (panelden düzenlenemez).</summary>
    public string? SystemAdminUsername { get; set; }
}

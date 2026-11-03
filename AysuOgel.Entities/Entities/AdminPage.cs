namespace AysuOgel.Entities.Entities;

/// <summary>Yönetim panelindeki bir sayfa (menü öğesi). Yöneticilere bu sayfalar üzerinden yetki verilir.</summary>
public class AdminPage
{
    public int Id { get; set; }

    /// <summary>Menüde ve yetki kartında görünen ad.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Yetki kartında başlığın altında görünen kısa açıklama.</summary>
    public string? Description { get; set; }

    /// <summary>Yetki kartındaki grup başlığı (ör. "İçerik", "Operasyon").</summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>Admin alanındaki controller adı (ör. "Campaigns"). Menü bağlantısı bu controller'ın Index'ine gider.</summary>
    public string Controller { get; set; } = string.Empty;

    /// <summary>Aynı yetkiyle erişilen yardımcı controller'lar, virgülle ayrılmış (ör. Kampanyalar için "Packages").</summary>
    public string? RelatedControllers { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AdminUserPage> AdminUsers { get; set; } = [];
}

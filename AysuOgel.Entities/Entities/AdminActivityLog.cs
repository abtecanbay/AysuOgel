namespace AysuOgel.Entities.Entities;

/// <summary>
/// Yönetim panelinde yapılan bir işlemin kaydı (ekleme, güncelleme, silme, giriş/çıkış...).
/// Yönetici silinse bile geçmiş korunsun diye kullanıcı bilgileri kayıt anındaki haliyle saklanır;
/// AdminUsers tablosuna yabancı anahtar yoktur.
/// </summary>
public class AdminActivityLog
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Panelden eklenen yöneticinin Id'si. Ana (sistem) yönetici için null.</summary>
    public int? AdminUserId { get; set; }

    /// <summary>İşlemi ana (sistem) yönetici mi yaptı?</summary>
    public bool IsSystemAdmin { get; set; }

    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>İşlemin yapıldığı sayfa (ör. "Kampanyalar", "Oturum").</summary>
    public string Category { get; set; } = string.Empty;

    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    /// <summary>İşlem yapılan kaydın kimliği (varsa).</summary>
    public string? EntityId { get; set; }

    /// <summary>İşlem yapılan kaydın adı/başlığı (formdan, varsa).</summary>
    public string? Target { get; set; }

    /// <summary>Okunabilir açıklama (ör. "Kampanya güncellendi.").</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>İşlem başarısız olduysa (ör. kayıt silinemedi) false.</summary>
    public bool IsSuccess { get; set; } = true;

    public string? IpAddress { get; set; }
}

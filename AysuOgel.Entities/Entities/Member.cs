namespace AysuOgel.Entities.Entities;

public class Member
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // E-posta ve telefondan en az biri dolu olmalı; ikisi de üye girişinde kullanıcı adı olarak kullanılabilir.

    /// <summary>Küçük harfe çevrilmiş halde saklanır. Boşsa null.</summary>
    public string? Email { get; set; }

    /// <summary>Yalnızca rakamlardan oluşur (9–12 hane). Boşsa null.</summary>
    public string? Phone { get; set; }

    /// <summary>PBKDF2 özeti; düz şifre hiçbir zaman saklanmaz.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public List<MemberPackage> Packages { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

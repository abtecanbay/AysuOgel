namespace AysuOgel.Entities.Entities;

public class Member
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Küçük harfe çevrilmiş halde saklanır; üye girişinde kullanıcı adı olarak kullanılacak.</summary>
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>PBKDF2 özeti; düz şifre hiçbir zaman saklanmaz.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public List<MemberPackage> Packages { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

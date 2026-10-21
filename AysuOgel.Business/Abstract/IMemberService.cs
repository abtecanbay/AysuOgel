using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

/// <summary>
/// Üye, satın alınan paket ve seans işlemleri. Hata döndüren metotlar
/// başarılıysa null, değilse kullanıcıya gösterilecek mesajı döner.
/// </summary>
public interface IMemberService
{
    Task<PagedResult<MemberListItem>> GetAllAsync(MemberFilter? filter = null);

    /// <summary>Paketler ve seanslarla birlikte.</summary>
    Task<Member?> GetByIdAsync(int id);

    Task<bool> EmailExistsAsync(string email, int? exceptMemberId = null);

    /// <summary>Üye kaydıyla birlikte ilk paketini de oluşturur.</summary>
    Task<Member> CreateAsync(Member member, string password, int packageId, DateOnly startDate, DateOnly? endDate);

    /// <summary>newPassword boşsa mevcut şifre korunur.</summary>
    Task UpdateAsync(Member member, string? newPassword);

    Task DeleteAsync(int id);

    /// <summary>E-posta ve şifre doğruysa üyeyi döner (üye seans sorgulama ekranı için).</summary>
    Task<Member?> VerifyCredentialsAsync(string email, string password);

    Task<MemberPackage?> GetMemberPackageAsync(int memberPackageId);
    Task<string?> AddPackageAsync(int memberId, int packageId, DateOnly startDate, DateOnly? endDate);
    Task<string?> UpdatePackageAsync(int memberPackageId, DateOnly startDate, DateOnly? endDate);
    Task DeletePackageAsync(int memberPackageId);

    /// <summary>Paket ve üye bilgisiyle birlikte.</summary>
    Task<MemberSession?> GetSessionAsync(int sessionId);

    /// <summary>
    /// Verilen saatin ±1 saat içindeki iptal edilmemiş seanslar (düzenlenen seans hariç).
    /// Kapasite yeterliyse yönetici onayıyla yine de kaydedilebilir.
    /// </summary>
    Task<List<SessionConflict>> GetConflictsAsync(DateTime scheduledAt, int? exceptSessionId = null);

    Task<string?> AddSessionAsync(MemberSession session);
    Task<string?> UpdateSessionAsync(MemberSession session);
    Task<string?> SetSessionStatusAsync(int sessionId, SessionStatus status);
    Task DeleteSessionAsync(int sessionId);
}

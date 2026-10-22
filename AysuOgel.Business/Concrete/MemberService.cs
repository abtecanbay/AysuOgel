using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class MemberService : IMemberService
{
    private readonly AppDbContext _db;
    public MemberService(AppDbContext db) => _db = db;

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public Task<PagedResult<MemberListItem>> GetAllAsync(MemberFilter? filter = null)
    {
        filter ??= new MemberFilter();
        var query = _db.Members.AsNoTracking().AsQueryable();

        var name = FilterHelper.Clean(filter.Name);
        if (name is not null)
            query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(name));

        var phone = FilterHelper.PhoneDigits(filter.Phone);
        if (phone is not null)
            query = query.Where(x => x.Phone
                .Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")
                .Contains(phone));

        var email = FilterHelper.Clean(filter.Email)?.ToLowerInvariant();
        if (email is not null)
            query = query.Where(x => x.Email.Contains(email));

        if (filter.PackageId is { } packageId)
            query = query.Where(x => x.Packages.Any(p => p.PackageId == packageId));

        if (filter.Status is { } status)
            query = query.Where(x => x.Packages.Any(p => p.Status == status));

        var (from, to) = (filter.StartFrom, filter.StartTo);
        if (from.HasValue && to.HasValue && from > to)
            (from, to) = (to, from);
        if (from is { } f)
            query = query.Where(x => x.Packages.Any(p => p.StartDate >= f));
        if (to is { } t)
            query = query.Where(x => x.Packages.Any(p => p.StartDate <= t));

        query = filter.Descending
            ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id);

        return query
            .Select(m => new
            {
                Member = m,
                Latest = m.Packages
                    .OrderByDescending(p => p.StartDate).ThenByDescending(p => p.Id)
                    .FirstOrDefault()
            })
            .Select(x => new MemberListItem
            {
                Id = x.Member.Id,
                FirstName = x.Member.FirstName,
                LastName = x.Member.LastName,
                Phone = x.Member.Phone,
                Email = x.Member.Email,
                CreatedAt = x.Member.CreatedAt,
                PackageCount = x.Member.Packages.Count,
                PackageName = x.Latest != null ? x.Latest.Package.Name : null,
                StartDate = x.Latest != null ? x.Latest.StartDate : null,
                EndDate = x.Latest != null ? x.Latest.EndDate : null,
                Status = x.Latest != null ? x.Latest.Status : null,
                TotalSessions = x.Latest != null ? x.Latest.SessionCount : 0,
                DoneSessions = x.Latest != null ? x.Latest.Sessions.Count(s => s.Status == SessionStatus.Yapildi) : 0
            })
            .ToPagedAsync(filter.Page, filter.PageSize);
    }

    public Task<Member?> GetByIdAsync(int id) =>
        _db.Members.AsNoTracking()
            .Include(x => x.Packages).ThenInclude(p => p.Package)
            .Include(x => x.Packages).ThenInclude(p => p.Sessions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> EmailExistsAsync(string email, int? exceptMemberId = null)
    {
        var normalized = NormalizeEmail(email);
        return _db.Members.AnyAsync(x => x.Email == normalized && x.Id != exceptMemberId);
    }

    public async Task<Member> CreateAsync(Member member, string password, int packageId, DateOnly startDate, DateOnly? endDate)
    {
        var package = await _db.Packages.FindAsync(packageId)
            ?? throw new InvalidOperationException("Paket bulunamadı.");

        member.Email = NormalizeEmail(member.Email);
        member.PasswordHash = PasswordHasher.Hash(password);
        member.CreatedAt = DateTime.UtcNow;
        member.Packages.Add(NewMemberPackage(package, startDate, endDate));

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task UpdateAsync(Member member, string? newPassword)
    {
        var entity = await _db.Members.FindAsync(member.Id);
        if (entity is null) return;

        entity.FirstName = member.FirstName;
        entity.LastName = member.LastName;
        entity.Email = NormalizeEmail(member.Email);
        entity.Phone = member.Phone;
        if (!string.IsNullOrEmpty(newPassword))
            entity.PasswordHash = PasswordHasher.Hash(newPassword);
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Members.FindAsync(id);
        if (entity is null) return;
        _db.Members.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // Kayıtlı olmayan e-postada da aynı süre harcansın; yanıt süresinden üye varlığı anlaşılmasın.
    private static readonly string DummyHash = PasswordHasher.Hash(Guid.NewGuid().ToString());

    public async Task<Member?> VerifyCredentialsAsync(string email, string password)
    {
        var normalized = NormalizeEmail(email);
        var member = await _db.Members.AsNoTracking().FirstOrDefaultAsync(x => x.Email == normalized);
        var ok = PasswordHasher.Verify(password, member?.PasswordHash ?? DummyHash);
        return member is not null && ok ? member : null;
    }

    // ---- Satın alınan paketler ----

    public Task<MemberPackage?> GetMemberPackageAsync(int memberPackageId) =>
        _db.MemberPackages.AsNoTracking()
            .Include(x => x.Package)
            .Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id == memberPackageId);

    public async Task<string?> AddPackageAsync(int memberId, int packageId, DateOnly startDate, DateOnly? endDate)
    {
        if (!await _db.Members.AnyAsync(x => x.Id == memberId)) return "Üye bulunamadı.";

        var package = await _db.Packages.FindAsync(packageId);
        if (package is null) return "Paket bulunamadı.";
        if (endDate.HasValue && endDate < startDate) return "Bitiş tarihi başlangıç tarihinden önce olamaz.";

        var memberPackage = NewMemberPackage(package, startDate, endDate);
        memberPackage.MemberId = memberId;
        _db.MemberPackages.Add(memberPackage);
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> UpdatePackageAsync(int memberPackageId, DateOnly startDate, DateOnly? endDate)
    {
        var entity = await _db.MemberPackages
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.Id == memberPackageId);
        if (entity is null) return "Paket kaydı bulunamadı.";

        if (endDate.HasValue && endDate < startDate) return "Bitiş tarihi başlangıç tarihinden önce olamaz.";

        var firstSession = entity.Sessions.Where(s => s.Status != SessionStatus.IptalEdildi)
            .Select(s => (DateTime?)s.ScheduledAt).Min();
        if (firstSession is { } first && DateOnly.FromDateTime(first) < startDate)
            return $"Başlangıç tarihi ilk seanstan ({first:dd.MM.yyyy}) sonra olamaz.";

        entity.StartDate = startDate;
        entity.EndDate = endDate;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task DeletePackageAsync(int memberPackageId)
    {
        var entity = await _db.MemberPackages.FindAsync(memberPackageId);
        if (entity is null) return;
        _db.MemberPackages.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private static MemberPackage NewMemberPackage(Package package, DateOnly startDate, DateOnly? endDate) => new()
    {
        PackageId = package.Id,
        SessionCount = package.SessionCount,
        StartDate = startDate,
        // Süreli pakette bitiş girilmemişse geçerlilik süresinden hesaplanır.
        EndDate = endDate ?? (package.ValidityDays is { } days ? startDate.AddDays(days) : null),
        Status = MemberPackageStatus.Aktif,
        CreatedAt = DateTime.UtcNow
    };

    // ---- Seanslar ----

    public Task<MemberSession?> GetSessionAsync(int sessionId) =>
        _db.MemberSessions.AsNoTracking()
            .Include(x => x.MemberPackage).ThenInclude(p => p.Package)
            .Include(x => x.MemberPackage).ThenInclude(p => p.Member)
            .FirstOrDefaultAsync(x => x.Id == sessionId);

    /// <summary>Bir seans yaklaşık 1 saat sürer; bu aralıktaki seanslar çakışma sayılır.</summary>
    public static readonly TimeSpan ConflictWindow = TimeSpan.FromHours(1);

    public Task<List<SessionConflict>> GetConflictsAsync(DateTime scheduledAt, int? exceptSessionId = null)
    {
        // Sınır hariç: 12:50'deki seans 13:50'ye yeni planı engellemez, 13:20'yi uyarır.
        var from = scheduledAt - ConflictWindow;
        var to = scheduledAt + ConflictWindow;

        return _db.MemberSessions.AsNoTracking()
            .Where(x => x.Status != SessionStatus.IptalEdildi
                     && x.ScheduledAt > from && x.ScheduledAt < to
                     && x.Id != exceptSessionId)
            .OrderBy(x => x.ScheduledAt)
            .Select(x => new SessionConflict
            {
                SessionId = x.Id,
                MemberId = x.MemberPackage.MemberId,
                MemberName = x.MemberPackage.Member.FirstName + " " + x.MemberPackage.Member.LastName,
                PackageName = x.MemberPackage.Package.Name,
                SessionNumber = x.SessionNumber,
                ScheduledAt = x.ScheduledAt,
                Status = x.Status
            })
            .ToListAsync();
    }

    public async Task<string?> AddSessionAsync(MemberSession session)
    {
        var memberPackage = await LoadPackageWithSessionsAsync(session.MemberPackageId);
        if (memberPackage is null) return "Paket kaydı bulunamadı.";

        var error = ValidateSession(memberPackage, session.ScheduledAt, session.Status);
        if (error is not null) return error;

        memberPackage.Sessions.Add(new MemberSession
        {
            ScheduledAt = session.ScheduledAt,
            Status = session.Status,
            Note = FilterHelper.Clean(session.Note),
            CreatedAt = DateTime.UtcNow
        });
        SessionsChanged(memberPackage);
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> UpdateSessionAsync(MemberSession session)
    {
        var entity = await _db.MemberSessions.FindAsync(session.Id);
        if (entity is null) return "Seans bulunamadı.";

        var memberPackage = await LoadPackageWithSessionsAsync(entity.MemberPackageId);
        if (memberPackage is null) return "Paket kaydı bulunamadı.";

        var error = ValidateSession(memberPackage, session.ScheduledAt, session.Status, entity.Id);
        if (error is not null) return error;

        entity.ScheduledAt = session.ScheduledAt;
        entity.Status = session.Status;
        entity.Note = FilterHelper.Clean(session.Note);
        entity.UpdatedAt = DateTime.UtcNow;

        SessionsChanged(memberPackage);
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> SetSessionStatusAsync(int sessionId, SessionStatus status)
    {
        var entity = await _db.MemberSessions.FindAsync(sessionId);
        if (entity is null) return "Seans bulunamadı.";

        var memberPackage = await LoadPackageWithSessionsAsync(entity.MemberPackageId);
        if (memberPackage is null) return "Paket kaydı bulunamadı.";

        var error = ValidateSession(memberPackage, entity.ScheduledAt, status, entity.Id);
        if (error is not null) return error;

        // Seans planlanan saatten önce yapıldı olarak işaretlenirse bunu nota düşer.
        var now = TurkeyNow();
        if (status == SessionStatus.Yapildi && entity.Status != SessionStatus.Yapildi && now < entity.ScheduledAt)
            entity.Note = AppendNote(entity.Note,
                $"{entity.ScheduledAt:dd.MM.yyyy HH:mm} tarihinde planlanmıştı, {now:dd.MM.yyyy HH:mm} tarihinde yapıldı.");

        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;

        SessionsChanged(memberPackage);
        await _db.SaveChangesAsync();
        return null;
    }

    private const int NoteMaxLength = 500;

    // Türkiye kalıcı olarak UTC+3 kullanıyor.
    private static DateTime TurkeyNow() => DateTime.UtcNow.AddHours(3);

    /// <summary>
    /// Mevcut notun sonuna ekler. Sığmazsa yeni not korunur, eski not kısaltılır.
    /// </summary>
    private static string AppendNote(string? existing, string addition)
    {
        existing = existing?.Trim();
        if (string.IsNullOrEmpty(existing)) return addition;

        var room = NoteMaxLength - addition.Length - 1;
        if (room <= 0) return addition;
        if (existing.Length > room) existing = existing[..(room - 1)].TrimEnd() + "…";

        return $"{existing} {addition}";
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        var entity = await _db.MemberSessions.FindAsync(sessionId);
        if (entity is null) return;

        var memberPackage = await LoadPackageWithSessionsAsync(entity.MemberPackageId);
        _db.MemberSessions.Remove(entity);
        if (memberPackage is not null)
        {
            memberPackage.Sessions.Remove(entity);
            SessionsChanged(memberPackage);
        }
        await _db.SaveChangesAsync();
    }

    private Task<MemberPackage?> LoadPackageWithSessionsAsync(int memberPackageId) =>
        _db.MemberPackages
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.Id == memberPackageId);

    private static string? ValidateSession(
        MemberPackage memberPackage, DateTime scheduledAt, SessionStatus status, int? exceptSessionId = null)
    {
        if (DateOnly.FromDateTime(scheduledAt) < memberPackage.StartDate)
            return $"Seans tarihi paket başlangıcından ({memberPackage.StartDate:dd.MM.yyyy}) önce olamaz.";

        if (status == SessionStatus.IptalEdildi) return null;

        // İptal edilen seanslar hakkı düşürmez.
        var activeOthers = memberPackage.Sessions
            .Count(s => s.Id != exceptSessionId && s.Status != SessionStatus.IptalEdildi);
        if (activeOthers >= memberPackage.SessionCount)
            return $"Bu paketin {memberPackage.SessionCount} seansının tamamı planlanmış.";

        return null;
    }

    private static void SessionsChanged(MemberPackage memberPackage)
    {
        Renumber(memberPackage);
        RefreshCompletion(memberPackage);
    }

    /// <summary>
    /// Seans numaraları tarih sırasından türetilir: en erken iptal edilmemiş seans 1. seanstır.
    /// Sonradan araya daha erken bir tarih planlanırsa numaralar kayar. İptal edilenler 0 alır.
    /// </summary>
    private static void Renumber(MemberPackage memberPackage)
    {
        var number = 0;
        foreach (var s in memberPackage.Sessions
                     .Where(s => s.Status != SessionStatus.IptalEdildi)
                     .OrderBy(s => s.ScheduledAt).ThenBy(s => s.CreatedAt))
        {
            number++;
            if (s.SessionNumber != number) s.SessionNumber = number;
        }

        foreach (var s in memberPackage.Sessions.Where(s => s.Status == SessionStatus.IptalEdildi && s.SessionNumber != 0))
            s.SessionNumber = 0;
    }

    /// <summary>
    /// Tüm seanslar yapıldıysa paketi tamamlar ve bitiş tarihini son seansın tarihi yapar.
    /// Yapılan seans geri alınırsa paket yeniden "devam ediyor" olur.
    /// </summary>
    private static void RefreshCompletion(MemberPackage memberPackage)
    {
        var done = memberPackage.Sessions.Where(s => s.Status == SessionStatus.Yapildi).ToList();

        if (done.Count >= memberPackage.SessionCount)
        {
            memberPackage.Status = MemberPackageStatus.Tamamlandi;
            memberPackage.EndDate = DateOnly.FromDateTime(done.Max(s => s.ScheduledAt));
            memberPackage.UpdatedAt = DateTime.UtcNow;
        }
        else if (memberPackage.Status == MemberPackageStatus.Tamamlandi)
        {
            memberPackage.Status = MemberPackageStatus.Aktif;
            memberPackage.UpdatedAt = DateTime.UtcNow;
        }
    }
}

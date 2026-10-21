using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Models;

/// <summary>Planlanmak istenen saate yakın (çakışma penceresi içindeki) başka bir seans.</summary>
public class SessionConflict
{
    public int SessionId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public int SessionNumber { get; set; }
    public DateTime ScheduledAt { get; set; }
    public SessionStatus Status { get; set; }
}

using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Models;

public class AppointmentFilter
{
    public AppointmentStatus? Status { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    /// <summary>Tarih aralığı yokken geçmiş randevuları da getirir.</summary>
    public bool All { get; set; }

    /// <summary>"asc" veya "desc". Boşsa: "geçmişi göster" açıksa yeniden eskiye, değilse eskiden yeniye.</summary>
    public string? Sort { get; set; }

    public bool Descending => Sort?.ToLowerInvariant() switch
    {
        "asc" => false,
        "desc" => true,
        _ => All
    };
}
using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Models;

public class MessageFilter
{
    public ContactSubject? Subject { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public DateOnly? AppointmentFrom { get; set; }
    public DateOnly? AppointmentTo { get; set; }

    /// <summary>"asc" veya "desc". Boşsa yeniden eskiye.</summary>
    public string? Sort { get; set; }

    public bool Descending => Sort?.ToLowerInvariant() switch
    {
        "asc" => false,
        _ => true
    };
}
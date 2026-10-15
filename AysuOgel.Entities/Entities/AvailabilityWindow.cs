namespace AysuOgel.Entities.Entities;

public class AvailabilityWindow
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }          // Türkiye tarihi
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }       // son randevunun bittiği saat
    public int SlotMinutes { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
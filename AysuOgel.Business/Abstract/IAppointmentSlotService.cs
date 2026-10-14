namespace AysuOgel.Business.Abstract;

public record SlotInfo(string Time, bool IsAvailable);

public interface IAppointmentSlotService
{
    DateOnly MinDate { get; }
    DateOnly MaxDate { get; }
    int[] ClosedWeekdays { get; }          // 0 = Pazar ... 6 = Cumartesi
    Task<List<SlotInfo>> GetSlotsAsync(DateOnly date);
    Task<bool> IsAvailableAsync(DateTime start);
}
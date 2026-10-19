using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IAvailabilityService
{
    Task<List<AvailabilityWindow>> GetUpcomingAsync();
    Task<AvailabilityWindow?> GetByIdAsync(int id);
    Task<bool> HasOverlapAsync(DateOnly date, TimeOnly start, TimeOnly end, int? excludeId = null);
    Task CreateAsync(AvailabilityWindow window);
    Task CreateRangeAsync(IReadOnlyCollection<AvailabilityWindow> windows);
    Task UpdateAsync(AvailabilityWindow window);
    Task DeleteAsync(int id);
}
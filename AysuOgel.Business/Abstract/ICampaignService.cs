using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface ICampaignService
{
    Task<List<Campaign>> GetCurrentAsync();
    Task<List<Campaign>> GetAllAsync();
    Task<Campaign?> GetByIdAsync(int id);
    Task CreateAsync(Campaign campaign);
    Task UpdateAsync(Campaign campaign);
    Task DeleteAsync(int id);
    Task<CampaignPageSettings> GetPageSettingsAsync();
    Task UpdatePageSettingsAsync(CampaignPageSettings settings);
}
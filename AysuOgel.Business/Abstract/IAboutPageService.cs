using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IAboutPageService
{
    Task<AboutPageContent> GetAsync();

    Task<AboutPageSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(AboutPageSettings settings);

    Task<List<AboutBlock>> GetAllBlocksAsync();
    Task<AboutBlock?> GetBlockByIdAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    Task CreateBlockAsync(AboutBlock block);
    Task UpdateBlockAsync(AboutBlock block);
    Task DeleteBlockAsync(int id);
}

using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface ISuccessStoryService
{
    Task<List<SuccessStory>> GetActiveAsync();
    Task<List<SuccessStory>> GetAllAsync();
    Task<SuccessStory?> GetByIdAsync(int id);
    Task CreateAsync(SuccessStory story);
    Task UpdateAsync(SuccessStory story);
    Task DeleteAsync(int id);
}
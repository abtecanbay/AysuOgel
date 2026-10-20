using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class SuccessStoryService : ISuccessStoryService
{
    private readonly AppDbContext _db;
    public SuccessStoryService(AppDbContext db) => _db = db;

    public Task<List<SuccessStory>> GetActiveAsync() =>
        _db.SuccessStories.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<SuccessStory>> GetAllAsync() =>
        _db.SuccessStories.AsNoTracking()
            .OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<SuccessStory?> GetByIdAsync(int id) =>
        _db.SuccessStories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task CreateAsync(SuccessStory story)
    {
        story.Description = story.Description.Trim();
        story.CreatedAt = DateTime.UtcNow;
        _db.SuccessStories.Add(story);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SuccessStory story)
    {
        var entity = await _db.SuccessStories.FindAsync(story.Id);
        if (entity is null) return;

        entity.BeforeImageUrl = story.BeforeImageUrl;
        entity.AfterImageUrl = story.AfterImageUrl;
        entity.Description = story.Description.Trim();
        entity.DisplayOrder = story.DisplayOrder;
        entity.IsActive = story.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.SuccessStories.FindAsync(id);
        if (entity is null) return;
        _db.SuccessStories.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
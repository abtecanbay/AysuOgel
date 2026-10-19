using AysuOgel.Business.Abstract;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class CampaignService : ICampaignService
{
    private readonly AppDbContext _db;

    public CampaignService(AppDbContext db) => _db = db;

    public Task<List<Campaign>> GetCurrentAsync()
    {
        // Türkiye yerel tarihi (UTC+3); kampanya tarihleri SQL date olarak saklanır.
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        return _db.Campaigns.AsNoTracking()
            .Where(x => x.IsActive && x.StartDate <= today && x.EndDate >= today)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.EndDate)
            .ToListAsync();
    }

    public Task<List<Campaign>> GetAllAsync() =>
        _db.Campaigns.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

    public Task<Campaign?> GetByIdAsync(int id) =>
        _db.Campaigns.FirstOrDefaultAsync(x => x.Id == id);

    public async Task CreateAsync(Campaign campaign)
    {
        campaign.CreatedAt = DateTime.UtcNow;
        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Campaign campaign)
    {
        var entity = await _db.Campaigns.FindAsync(campaign.Id);
        if (entity is null) return;

        entity.Title = campaign.Title;
        entity.ShortDescription = campaign.ShortDescription;
        entity.Description = campaign.Description;
        entity.ImageUrl = campaign.ImageUrl;
        entity.StartDate = campaign.StartDate;
        entity.EndDate = campaign.EndDate;
        entity.IsActive = campaign.IsActive;
        entity.DisplayOrder = campaign.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Campaigns.FindAsync(id);
        if (entity is null) return;

        _db.Campaigns.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<CampaignPageSettings> GetPageSettingsAsync()
    {
        var settings = await _db.CampaignPageSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (settings is not null) return settings;

        settings = new CampaignPageSettings { Id = 1 };
        _db.CampaignPageSettings.Add(settings);
        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task UpdatePageSettingsAsync(CampaignPageSettings settings)
    {
        var entity = await _db.CampaignPageSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (entity is null)
        {
            settings.Id = 1;
            settings.UpdatedAt = DateTime.UtcNow;
            _db.CampaignPageSettings.Add(settings);
        }
        else
        {
            entity.BannerImageUrl = settings.BannerImageUrl;
            entity.IntroTitle = settings.IntroTitle;
            entity.IntroText = settings.IntroText;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
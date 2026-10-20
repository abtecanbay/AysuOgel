using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

public class AboutPageService : IAboutPageService
{
    private readonly AppDbContext _db;
    public AboutPageService(AppDbContext db) => _db = db;

    public async Task<AboutPageContent> GetAsync()
    {
        var settings = await _db.AboutPageSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1)
                       ?? new AboutPageSettings();

        var blocks = await _db.AboutBlocks.AsNoTracking()
            .Include(x => x.Images)
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
            .ToListAsync();

        return new AboutPageContent
        {
            Title = settings.Title,
            Intro = settings.Intro,
            Blocks = blocks.Select(b => new ContentBlock
            {
                Slug = b.Slug,
                Title = b.Title,
                Text = b.Text,
                Layout = (BlockLayout)(int)b.Layout,
                Images = b.Images
                    .OrderBy(i => i.DisplayOrder).ThenBy(i => i.Id)
                    .Select(i => new ImageItem { Url = i.Url, Alt = i.Alt ?? b.Title, Caption = i.Caption })
                    .ToList()
            }).ToList()
        };
    }

    public async Task<AboutPageSettings> GetSettingsAsync()
    {
        var settings = await _db.AboutPageSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (settings is not null) return settings;

        settings = new AboutPageSettings { Id = 1 };
        _db.AboutPageSettings.Add(settings);
        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task UpdateSettingsAsync(AboutPageSettings settings)
    {
        var entity = await _db.AboutPageSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (entity is null)
        {
            settings.Id = 1;
            settings.UpdatedAt = DateTime.UtcNow;
            _db.AboutPageSettings.Add(settings);
        }
        else
        {
            entity.Title = settings.Title.Trim();
            entity.Intro = settings.Intro?.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public Task<List<AboutBlock>> GetAllBlocksAsync() =>
        _db.AboutBlocks.AsNoTracking()
            .Include(x => x.Images)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
            .ToListAsync();

    public async Task<AboutBlock?> GetBlockByIdAsync(int id)
    {
        var block = await _db.AboutBlocks.AsNoTracking()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        block?.Images.Sort((a, b) => a.DisplayOrder != b.DisplayOrder
            ? a.DisplayOrder.CompareTo(b.DisplayOrder)
            : a.Id.CompareTo(b.Id));
        return block;
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null) =>
        _db.AboutBlocks.AnyAsync(x => x.Slug == slug && (excludeId == null || x.Id != excludeId));

    public async Task CreateBlockAsync(AboutBlock block)
    {
        Normalize(block);
        block.CreatedAt = DateTime.UtcNow;
        _db.AboutBlocks.Add(block);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateBlockAsync(AboutBlock block)
    {
        var entity = await _db.AboutBlocks.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == block.Id);
        if (entity is null) return;

        Normalize(block);
        entity.Slug = block.Slug;
        entity.Title = block.Title;
        entity.Text = block.Text;
        entity.Layout = block.Layout;
        entity.DisplayOrder = block.DisplayOrder;
        entity.IsActive = block.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        // Gelen liste görsellerin son halidir: listede olmayanlar silinir, Id'si 0 olanlar eklenir.
        var incoming = block.Images.Where(i => i.Id > 0).ToDictionary(i => i.Id);
        foreach (var image in entity.Images.ToList())
        {
            if (incoming.TryGetValue(image.Id, out var changed))
            {
                image.Alt = changed.Alt;
                image.Caption = changed.Caption;
                image.DisplayOrder = changed.DisplayOrder;
            }
            else
            {
                _db.AboutBlockImages.Remove(image);
            }
        }

        foreach (var image in block.Images.Where(i => i.Id == 0))
            entity.Images.Add(new AboutBlockImage
            {
                Url = image.Url,
                Alt = image.Alt,
                Caption = image.Caption,
                DisplayOrder = image.DisplayOrder
            });

        await _db.SaveChangesAsync();
    }

    public async Task DeleteBlockAsync(int id)
    {
        var entity = await _db.AboutBlocks.FindAsync(id);
        if (entity is null) return;
        _db.AboutBlocks.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private static void Normalize(AboutBlock block)
    {
        block.Title = block.Title.Trim();
        block.Text = FilterHelper.Clean(block.Text);
        block.Slug = string.IsNullOrWhiteSpace(block.Slug) ? SlugHelper.ToSlug(block.Title) : block.Slug.Trim();

        foreach (var image in block.Images)
        {
            image.Alt = FilterHelper.Clean(image.Alt);
            image.Caption = FilterHelper.Clean(image.Caption);
        }
    }
}

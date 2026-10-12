using AysuOgel.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.DataAccess.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services => Set<Service>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Services");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ShortDescription)
                .HasMaxLength(300);

            entity.Property(x => x.Description)
                .HasMaxLength(3000);

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(1000);

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.DisplayOrder
            });
        });
    }
}
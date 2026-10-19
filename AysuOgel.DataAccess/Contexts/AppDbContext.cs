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
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AvailabilityWindow> AvailabilityWindows => Set<AvailabilityWindow>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MessageReply> MessageReplies => Set<MessageReply>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignPageSettings> CampaignPageSettings => Set<CampaignPageSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns", table =>
                table.HasCheckConstraint("CK_Campaigns_DateRange", "[EndDate] >= [StartDate]"));
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ShortDescription).HasMaxLength(300);
            entity.Property(x => x.Description).HasMaxLength(3000).IsRequired();
            entity.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.EndDate).HasColumnType("date").IsRequired();

            entity.HasIndex(x => new { x.IsActive, x.StartDate, x.EndDate, x.DisplayOrder });
        });

        modelBuilder.Entity<CampaignPageSettings>(entity =>
        {
            entity.ToTable("CampaignPageSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.BannerImageUrl).HasMaxLength(1000);
            entity.Property(x => x.IntroTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.IntroText).HasMaxLength(1000).IsRequired();
        });

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

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.ToTable("ContactMessages");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();

            entity.HasIndex(x => new { x.IsRead, x.CreatedAt });
        });

        modelBuilder.Entity<AvailabilityWindow>(entity =>
        {
            entity.ToTable("AvailabilityWindows");
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.Date, x.StartTime });
        });

        modelBuilder.Entity<MessageReply>(entity =>
        {
            entity.ToTable("MessageReplies");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ToEmail).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(500);

            entity.HasOne(x => x.ContactMessage)
                .WithMany()
                .HasForeignKey(x => x.ContactMessageId)
                .OnDelete(DeleteBehavior.Cascade);   // mesaj silinirse yanıtları da silinir

            entity.HasIndex(x => new { x.ContactMessageId, x.CreatedAt });
            entity.HasIndex(x => new { x.DeliveryStatus, x.CreatedAt });   // ileride bekleyenleri göndermek için
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.ContactMessage)
                .WithMany()
                .HasForeignKey(x => x.ContactMessageId)
                .OnDelete(DeleteBehavior.Cascade);   // mesaj silinirse randevu da silinir, saat boşalır

            // İptal edilmemiş her saat için tek kayıt: aynı saate çift randevuyu DB engeller.
            entity.HasIndex(x => x.SlotStart)
                .IsUnique()
                .HasFilter($"[Status] <> {(int)AppointmentStatus.IptalEdildi}");

            entity.HasIndex(x => new { x.Status, x.SlotStart });
        });
    }
}
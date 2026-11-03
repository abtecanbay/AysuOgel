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
    public DbSet<SuccessStory> SuccessStories => Set<SuccessStory>();
    public DbSet<AboutPageSettings> AboutPageSettings => Set<AboutPageSettings>();
    public DbSet<AboutBlock> AboutBlocks => Set<AboutBlock>();
    public DbSet<AboutBlockImage> AboutBlockImages => Set<AboutBlockImage>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberPackage> MemberPackages => Set<MemberPackage>();
    public DbSet<MemberSession> MemberSessions => Set<MemberSession>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AdminPage> AdminPages => Set<AdminPage>();
    public DbSet<AdminUserPage> AdminUserPages => Set<AdminUserPage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.ToTable("SiteSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.SiteTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BrandName).HasMaxLength(60).IsRequired();
            entity.Property(x => x.LogoUrl).HasMaxLength(1000);
            entity.Property(x => x.FaviconUrl).HasMaxLength(1000);

            entity.HasData(new SiteSettings
            {
                Id = 1,
                SiteTitle = "Aysu Öğel Sağlıklı Yaşam",
                BrandName = "AysuOgel"
            });
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("AdminUsers");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();

            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<AdminPage>(entity =>
        {
            entity.ToTable("AdminPages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Title).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.Property(x => x.GroupName).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Controller).HasMaxLength(80).IsRequired();
            entity.Property(x => x.RelatedControllers).HasMaxLength(300);

            entity.HasIndex(x => x.Controller).IsUnique();

            entity.HasData(
                new AdminPage { Id = 1, SortOrder = 1, GroupName = "İçerik", Title = "Hizmetler", Controller = "Services", Description = "Hizmet kartlarını ekleme, düzenleme ve sıralama." },
                new AdminPage { Id = 2, SortOrder = 2, GroupName = "İçerik", Title = "Kampanyalar", Controller = "Campaigns", RelatedControllers = "Packages", Description = "Kampanyalar, paketler ve kampanya sayfası ayarları." },
                new AdminPage { Id = 3, SortOrder = 3, GroupName = "İçerik", Title = "Başarılarımız", Controller = "SuccessStories", Description = "Başarı hikâyeleri ve önce/sonra görselleri." },
                new AdminPage { Id = 4, SortOrder = 4, GroupName = "İçerik", Title = "Hakkımızda", Controller = "AboutPage", Description = "Hakkımızda sayfasının blokları ve ayarları." },
                new AdminPage { Id = 5, SortOrder = 5, GroupName = "Operasyon", Title = "Üyeler", Controller = "Members", Description = "Üye kayıtları, paketleri ve seansları." },
                new AdminPage { Id = 6, SortOrder = 6, GroupName = "Operasyon", Title = "Mesajlar", Controller = "Messages", Description = "İletişim formundan gelen mesajlar ve yanıtlar." },
                new AdminPage { Id = 7, SortOrder = 7, GroupName = "Operasyon", Title = "Randevular", Controller = "Appointments", Description = "Gelen randevu talepleri." },
                new AdminPage { Id = 8, SortOrder = 8, GroupName = "Operasyon", Title = "Müsaitlik", Controller = "Availability", Description = "Randevu alınabilecek müsaitlik saatleri." },
                new AdminPage { Id = 9, SortOrder = 9, GroupName = "Sistem", Title = "Sayfa Yönetimi", Controller = "SiteManagement", Description = "Site ayarları ve yönetici hesapları (yetki verebilir)." });
        });

        modelBuilder.Entity<AdminUserPage>(entity =>
        {
            entity.ToTable("AdminUserPages");
            entity.HasKey(x => new { x.AdminUserId, x.AdminPageId });

            entity.HasOne(x => x.AdminUser)
                .WithMany(x => x.Pages)
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.AdminPage)
                .WithMany(x => x.AdminUsers)
                .HasForeignKey(x => x.AdminPageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.ToTable("Packages");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Price).HasPrecision(12, 2);

            entity.HasIndex(x => new { x.IsActive, x.DisplayOrder });
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Members");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();   // üye girişi e-posta ile yapılacak
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<MemberPackage>(entity =>
        {
            entity.ToTable("MemberPackages", table =>
                table.HasCheckConstraint("CK_MemberPackages_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]"));
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.EndDate).HasColumnType("date");

            entity.HasOne(x => x.Member)
                .WithMany(x => x.Packages)
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Cascade);   // üye silinirse paketleri de silinir

            entity.HasOne(x => x.Package)
                .WithMany()
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);  // satılmış paket silinemez, pasife alınır

            entity.HasIndex(x => new { x.MemberId, x.StartDate });
        });

        modelBuilder.Entity<MemberSession>(entity =>
        {
            entity.ToTable("MemberSessions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Note).HasMaxLength(500);

            entity.HasOne(x => x.MemberPackage)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.MemberPackageId)
                .OnDelete(DeleteBehavior.Cascade);   // paket kaydı silinirse seansları da silinir

            entity.HasIndex(x => new { x.MemberPackageId, x.SessionNumber });
            entity.HasIndex(x => new { x.Status, x.ScheduledAt });
        });

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

            entity.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.SetNull);   // üye silinse de mesaj kalır
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

        modelBuilder.Entity<SuccessStory>(entity =>
        {
            entity.ToTable("SuccessStories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BeforeImageUrl).HasMaxLength(1000);
            entity.Property(x => x.AfterImageUrl).HasMaxLength(1000);
            entity.Property(x => x.Description).HasMaxLength(600).IsRequired();

            entity.HasIndex(x => new { x.IsActive, x.DisplayOrder });
        });

        modelBuilder.Entity<AboutPageSettings>(entity =>
        {
            entity.ToTable("AboutPageSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Intro).HasMaxLength(1000);

            entity.HasData(new AboutPageSettings
            {
                Id = 1,
                Title = "Hakkımızda",
                Intro = "Çankaya, Ankara'da 6 yıldır hizmet veriyoruz. Bu alan örnek metindir; işletmenin kısa tanıtımı buraya gelecek."
            });
        });

        modelBuilder.Entity<AboutBlock>(entity =>
        {
            entity.ToTable("AboutBlocks");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Text).HasMaxLength(3000);

            entity.HasIndex(x => x.Slug).IsUnique();   // sayfa içi bağlantılar (#post-{Slug}) çakışmasın
            entity.HasIndex(x => new { x.IsActive, x.DisplayOrder });

            // Mevcut statik içerik, migration sonrası sayfa boş kalmasın diye taşınır.
            var seededAt = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
            entity.HasData(
                new AboutBlock
                {
                    Id = 1, Slug = "mekanimiz", Title = "Mekanımız", DisplayOrder = 1, IsActive = true, CreatedAt = seededAt,
                    Text = "Bu alan örnek metindir.\nMekanın genel tanıtımı, yaklaşımınız ve hizmet anlayışınız buraya yazılabilir.",
                    Layout = AboutBlockLayout.ImageLeft
                },
                new AboutBlock
                {
                    Id = 2, Slug = "vaccu-active", Title = "Vaccu Active", DisplayOrder = 2, IsActive = true, CreatedAt = seededAt,
                    Text = "Bu alan örnek metindir.\nUygulamanın ne olduğu, nasıl yapıldığı ve seans bilgileri buraya yazılabilir.",
                    Layout = AboutBlockLayout.ImageRight
                },
                new AboutBlock
                {
                    Id = 3, Slug = "roll-shape", Title = "Roll Shape", DisplayOrder = 3, IsActive = true, CreatedAt = seededAt,
                    Text = "Bu alan örnek metindir.\nUygulamanın tanıtımı ve seans bilgileri buraya yazılabilir.",
                    Layout = AboutBlockLayout.ImageLeft
                },
                new AboutBlock
                {
                    Id = 4, Slug = "cilt-bakimi", Title = "Profesyonel Cilt Bakımı", DisplayOrder = 4, IsActive = true, CreatedAt = seededAt,
                    Text = "Bu alan örnek metindir.\nCilt bakımı hizmetleri ve kullanılan ürünler buraya yazılabilir.",
                    Layout = AboutBlockLayout.ImageRight
                });
        });

        modelBuilder.Entity<AboutBlockImage>(entity =>
        {
            entity.ToTable("AboutBlockImages");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Url).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Alt).HasMaxLength(200);
            entity.Property(x => x.Caption).HasMaxLength(300);

            entity.HasOne(x => x.AboutBlock)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.AboutBlockId)
                .OnDelete(DeleteBehavior.Cascade);   // blok silinirse görsel kayıtları da silinir

            entity.HasIndex(x => new { x.AboutBlockId, x.DisplayOrder });
        });
    }
}
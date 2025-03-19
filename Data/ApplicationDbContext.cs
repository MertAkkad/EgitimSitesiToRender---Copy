using EgitimSitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EgitimSitesi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BannerModel> Banners { get; set; }
        public DbSet<Anasayfa_IcerikModel> AnasayfaIcerik { get; set; }
        public DbSet<DuyurularModel> Duyurular { get; set; }
        public DbSet<KadromuzModel> Kadromuz { get; set; }
        public DbSet<SubeModel> Subeler { get; set; }
        public DbSet<HakkimizdaModel> Hakkimizda { get; set; }
        public DbSet<KayitFormuModel> KayitFormu { get; set; }
        public DbSet<IletisimModel> Iletisim { get; set; }
        public DbSet<BizeYazinModel> BizeYazin { get; set; }
        public DbSet<SiteSettingsModel> SiteSettings { get; set; }
        public DbSet<KursModel> Kurslar { get; set; }
        public DbSet<GalleryModel> Gallery { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Suppress the pending model changes warning
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BannerModel
            modelBuilder.Entity<BannerModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImagePath).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ButtonText).HasMaxLength(50);
                entity.Property(e => e.ButtonUrl).HasMaxLength(200);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add index for frequently queried fields
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Order);
            });

            // Configure DuyurularModel
            modelBuilder.Entity<DuyurularModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImagePath).HasMaxLength(200);
                entity.Property(e => e.IconClass).HasMaxLength(50);
                entity.Property(e => e.ButtonText).HasMaxLength(50);
                entity.Property(e => e.ButtonUrl).HasMaxLength(200);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.AnnouncementType).HasMaxLength(50);
                
                // Add composite index for the most common query pattern (finding active announcements ordered by date)
                entity.HasIndex(e => new { e.IsActive, e.CreationDate });
            });
            
            // Configure KadromuzModel
            modelBuilder.Entity<KadromuzModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Grade).IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Field).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Exp).HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.ImagePath).HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add indexes for common search and filter operations
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Field);
            });
            
            // Configure SubeModel
            modelBuilder.Entity<SubeModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TelNo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.WorkHours).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Latitude).IsRequired();
                entity.Property(e => e.Longitude).IsRequired();
                entity.Property(e => e.MapUrl).HasMaxLength(500);
                entity.Property(e => e.ZoomLevel).HasDefaultValue(15);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add spatial index for location-based queries (if your DB supports it)
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
            });
            
            // Configure HakkimizdaModel
            modelBuilder.Entity<HakkimizdaModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImagePath).HasMaxLength(200);
                entity.Property(e => e.Tarihcemiz).IsRequired();
                entity.Property(e => e.Vizyonumuz).IsRequired();
                entity.Property(e => e.Misyonumuz).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add index for active status
                entity.HasIndex(e => e.IsActive);
            });

            // Configure KayitFormuModel
            modelBuilder.Entity<KayitFormuModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TelNo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Grade).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add indexes for common search patterns
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.CreationDate);
            });

            // Configure IletisimModel
            modelBuilder.Entity<IletisimModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MerkezSube).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Adress).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TelNo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.GoogleMapsEmbed).HasMaxLength(1000);
                entity.Property(e => e.CalismaSaatleri).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add index for active status
                entity.HasIndex(e => e.IsActive);
            });
            
            // Configure BizeYazinModel
            modelBuilder.Entity<BizeYazinModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AdSoyad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TelefonNo).HasMaxLength(20);
                entity.Property(e => e.Konu).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Mesaj).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Okundu).HasDefaultValue(false);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IpAdresi).HasMaxLength(50);
                
                // Add indexes for common filtering
                entity.HasIndex(e => e.Okundu);
                entity.HasIndex(e => e.CreationDate);
            });

            // Configure SiteSettingsModel
            modelBuilder.Entity<SiteSettingsModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActiveLayout).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add index for frequent lookups (since this is likely to be queried on every page load)
                entity.HasIndex(e => e.ActiveLayout);
            });
            
            // Configure KursModel
            modelBuilder.Entity<KursModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Details).HasColumnType("text");
                entity.Property(e => e.ImagePath).HasMaxLength(200);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add indexes for filtering and sorting
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => new { e.IsActive, e.Order });
            });
            
            // Configure GalleryModel
            modelBuilder.Entity<GalleryModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImagePath).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add indexes for common query patterns
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => new { e.IsActive, e.Type, e.Order });
            });
        }
    }
} 
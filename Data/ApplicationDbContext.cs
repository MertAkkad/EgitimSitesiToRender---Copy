using EgitimSitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        // Override SaveChanges to convert all DateTime properties to UTC before saving
        public override int SaveChanges()
        {
            ConvertDateTimesToUtc();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ConvertDateTimesToUtc();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        private void ConvertDateTimesToUtc()
        {
            // Get all entities with DateTime properties that have been added or modified
            var entityEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entityEntries)
            {
                // Find all DateTime properties
                var dateTimeProperties = entityEntry.Entity.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var property in dateTimeProperties)
                {
                    // Get the current value
                    var value = property.GetValue(entityEntry.Entity);

                    if (value != null)
                    {
                        if (property.PropertyType == typeof(DateTime))
                        {
                            var dateTime = (DateTime)value;
                            // Only convert if it's not already UTC
                            if (dateTime.Kind != DateTimeKind.Utc)
                            {
                                property.SetValue(entityEntry.Entity, DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
                            }
                        }
                        else if (property.PropertyType == typeof(DateTime?))
                        {
                            var nullableDateTime = (DateTime?)value;
                            if (nullableDateTime.HasValue && nullableDateTime.Value.Kind != DateTimeKind.Utc)
                            {
                                property.SetValue(entityEntry.Entity, DateTime.SpecifyKind(nullableDateTime.Value, DateTimeKind.Utc));
                            }
                        }
                    }
                }
            }
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
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
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

            // Configure Anasayfa_IcerikModel
            modelBuilder.Entity<Anasayfa_IcerikModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ButtonText).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ButtonUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.IconClass).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImagePath).IsRequired();
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsImageRight).HasDefaultValue(false);
            });

            // Configure DuyurularModel
            modelBuilder.Entity<DuyurularModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImagePath).HasMaxLength(200);
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
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
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
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
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
                entity.Property(e => e.Tarihcemiz).IsRequired();
                entity.Property(e => e.Vizyonumuz).IsRequired();
                entity.Property(e => e.Misyonumuz).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Add index for active status
                entity.HasIndex(e => e.IsActive);
            });

            // Configure SiteSettingsModel
            modelBuilder.Entity<SiteSettingsModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActiveLayout).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            // Configure KursModel
            modelBuilder.Entity<KursModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImagePath).HasMaxLength(200);
                entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Details).IsRequired();
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
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
        }
    }
} 
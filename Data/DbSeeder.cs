using System;
using System.Linq;
using System.Threading.Tasks;
using EgitimSitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EgitimSitesi.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await SeedBanners(context);
                await SeedAnasayfaIcerik(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
            }
        }

        private static async Task SeedBanners(ApplicationDbContext context)
        {
            // Only seed if the database is empty
            if (await context.Banners.AnyAsync())
            {
                return;
            }

            // Add sample banners
            var banners = new[]
            {
                new BannerModel
                {
                    Title = "Geleceğe Hazırlık",
                    Description = "Kaliteli eğitim ile öğrencilerimizi geleceğe hazırlıyoruz",
                    ImagePath = "/images/banners/banner1.jpg",
                    ButtonText = "Detaylı Bilgi",
                    ButtonUrl = "/Hakkimizda",
                    Order = 1,
                    IsActive = true,
                    CreationDate = DateTime.Now
                },
                new BannerModel
                {
                    Title = "Deneyimli Kadro",
                    Description = "Alanında uzman eğitimcilerimizle başarıya ulaşın",
                    ImagePath = "/images/banners/banner2.jpg",
                    ButtonText = "Kadromuz",
                    ButtonUrl = "/Kadromuz",
                    Order = 2,
                    IsActive = true,
                    CreationDate = DateTime.Now
                },
                new BannerModel
                {
                    Title = "Özel Program",
                    Description = "Kişiselleştirilmiş eğitim programları ile fark yaratın",
                    ImagePath = "/images/banners/banner3.jpg",
                    ButtonText = "Kurslarımız",
                    ButtonUrl = "/Kurslarimiz",
                    Order = 3,
                    IsActive = true,
                    CreationDate = DateTime.Now
                }
            };

            await context.Banners.AddRangeAsync(banners);
            await context.SaveChangesAsync();
        }
        
        private static async Task SeedAnasayfaIcerik(ApplicationDbContext context)
        {
            // Only seed if the database is empty
            if (await context.AnasayfaIcerik.AnyAsync())
            {
                return;
            }

            // Add sample content items with alternating image positions
            var contentItems = new[]
            {
                new Anasayfa_IcerikModel
                {
                    Title = "Uzman Eğitim Kadrosu",
                    Description = "Kurumumuzda alanında uzman, yılların deneyimine sahip öğretmenlerimiz görev yapmaktadır. Öğrencilerimizin akademik başarısını en üst seviyeye çıkarmak için sürekli kendilerini geliştiren öğretmenlerimiz, modern eğitim tekniklerini kullanarak öğrencilerimizin potansiyellerini en iyi şekilde ortaya çıkarmaktadır.",
                    ImagePath = "https://images.unsplash.com/photo-1594608661623-aa0bd3a69465?q=80&w=1548&auto=format&fit=crop",
                    IconClass = "fas fa-user-graduate",
                    ButtonText = "Kadromuzu Tanıyın",
                    ButtonUrl = "/Kadromuz",
                    Order = 1,
                    IsActive = true,
                    IsImageRight = false,
                    CreationDate = DateTime.Now
                },
                new Anasayfa_IcerikModel
                {
                    Title = "Kişiselleştirilmiş Eğitim Programları",
                    Description = "Her öğrencinin öğrenme stili ve ihtiyaçları farklıdır. Bu nedenle eğitim programlarımızı kişiselleştirerek, her öğrencinin kendi hızında ilerlemesini ve potansiyelini maksimum düzeyde kullanmasını sağlıyoruz. Düzenli ölçme-değerlendirme sistemimiz ile öğrencilerimizin gelişimini takip ediyor ve gerekli yönlendirmeleri yapıyoruz.",
                    ImagePath = "https://images.unsplash.com/photo-1576267423445-b2e0074d68a4?q=80&w=1470&auto=format&fit=crop",
                    IconClass = "fas fa-book-open",
                    ButtonText = "Programlarımız Hakkında",
                    ButtonUrl = "/Kurslarimiz",
                    Order = 2,
                    IsActive = true,
                    IsImageRight = true,
                    CreationDate = DateTime.Now
                },
                new Anasayfa_IcerikModel
                {
                    Title = "Modern Teknolojik Altyapı",
                    Description = "Dersliklerimiz ve çalışma alanlarımız, modern eğitim teknolojileri ile donatılmıştır. Akıllı tahtalar, bilgisayar laboratuvarları ve dijital kütüphanemiz ile öğrencilerimizin eğitim kalitesini artırıyor, onları geleceğin dünyasına hazırlıyoruz. Teknolojik altyapımız sayesinde online eğitim imkanı da sunmaktayız.",
                    ImagePath = "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?q=80&w=1471&auto=format&fit=crop",
                    IconClass = "fas fa-laptop",
                    ButtonText = "Teknolojik Altyapımız",
                    ButtonUrl = "/Hakkimizda",
                    Order = 3,
                    IsActive = true,
                    IsImageRight = false,
                    CreationDate = DateTime.Now
                }
            };

            await context.AnasayfaIcerik.AddRangeAsync(contentItems);
            await context.SaveChangesAsync();
        }
    }
} 
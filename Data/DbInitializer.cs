using EgitimSitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EgitimSitesi.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();
            
            try
            {
                // For PostgreSQL, ensure the database exists
                await dbContext.Database.EnsureCreatedAsync();
                
                // In production, we only want to ensure the SiteSettings exists
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Production")
                {
                    await EnsureSiteSettingsExistsAsync(dbContext);
                }
                else
                {
                    // For development, ensure SiteSettings exists
                    await EnsureSiteSettingsExistsAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while initializing the database.");
                // Continue execution - don't let migration errors stop the application
            }
        }
        
        private static async Task EnsureSiteSettingsExistsAsync(ApplicationDbContext dbContext)
        {
            try
            {
                // Check if any SiteSettings record exists
                if (!await dbContext.SiteSettings.AnyAsync())
                {
                    // Create default SiteSettings
                    var defaultSettings = new SiteSettingsModel
                    {
                        ActiveLayout = "_Layout",
                        ImagePath = "/images/default-logo.png",
                        CreationDate = DateTime.Now
                    };
                    
                    dbContext.SiteSettings.Add(defaultSettings);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch
            {
                // Silently fail - the table may not exist yet
            }
        }
    }
} 
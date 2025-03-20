using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using EgitimSitesi.Models;
using CloudinaryDotNet;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add database context - Check if we're using environment variables or connection string
if (Environment.GetEnvironmentVariable("PGHOST") != null)
{
    // Build connection string from environment variables
    var pgUser = Environment.GetEnvironmentVariable("PGUSER");
    var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
    var pgHost = Environment.GetEnvironmentVariable("PGHOST");
    var pgPort = Environment.GetEnvironmentVariable("PGPORT");
    var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
    
    var connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use connection string from appsettings.json
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Configure Cloudinary - Check for environment variables first
if (Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") != null)
{
    builder.Services.AddSingleton(provider => {
        return new Cloudinary(new Account(
            Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
            Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
            Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")));
    });
}
else
{
    // Use appsettings.json config
    builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
    builder.Services.AddSingleton(provider => {
        var config = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CloudinarySettings>>().Value;
        return new Cloudinary(new Account(
            config.CloudName,
            config.ApiKey,
            config.ApiSecret));
    });
}

builder.Services.AddScoped<EgitimSitesi.Services.CloudinaryService>();

var app = builder.Build();

// Apply database migrations automatically
app.MigrateDatabase();

// Initialize the database and ensure default data exists
await EgitimSitesi.Data.DbInitializer.Initialize(app.Services);

// Apply migrations and seed the database only in development
if (app.Environment.IsDevelopment())
{
    // Call the seeder
    await DbSeeder.SeedDatabase(app);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Map root and /Home to Anasayfa controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Anasayfa}/{action=Index}/{id?}",
    defaults: new { controller = "Anasayfa" })
    .WithStaticAssets();

// Home controllers in HomeControllers folder
app.MapControllerRoute(
    name: "homeControllers",
    pattern: "{controller}/{action=Index}/{id?}",
    defaults: new { area = "" },
    constraints: new { controller = @"(Anasayfa|Hakkimizda|Kadromuz|Subelerimiz|BasvuruFormu|Iletisim)" })
    .WithStaticAssets();

// Course controllers in HomeControllers/Kurslar folder
app.MapControllerRoute(
    name: "kurslarControllers",
    pattern: "Kurslar/{controller=Kurslar}/{action=Index}/{id?}",
    defaults: new { area = "" })
    .WithStaticAssets();

await app.RunAsync();

// Extension method to automatically apply database migrations
public static class MigrationExtensions
{
    public static IApplicationBuilder MigrateDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                try
                {
                    context.Database.Migrate();
                    app.Logger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "An error occurred while migrating the database");
                }
            }
        }
        return app;
    }
}

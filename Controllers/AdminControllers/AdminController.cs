using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using EgitimSitesi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using EgitimSitesi.Data;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using EgitimSitesi.Services;
using Microsoft.Extensions.Configuration;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public AdminController(
            IConfiguration configuration, 
            ApplicationDbContext context,
            CloudinaryService cloudinaryService)
        {
            _configuration = configuration;
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: /Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get active layout
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            string activeLayout = siteSettings?.ActiveLayout ?? "_Layout";
            
            // Fetch actual counts from database
            var adminDashboard = new AdminDashboardViewModel
            {
                BannerCount = await _context.Banners.CountAsync(),
                IcerikCount = await _context.AnasayfaIcerik.CountAsync(),
                KurslarCount = await _context.Kurslar.CountAsync(),
                DuyurularCount = await _context.Duyurular.CountAsync(),
                KadromuzCount = await _context.Kadromuz.CountAsync(),
                SubelerCount = await _context.Subeler.CountAsync(),
                HakkimizdaCount = await _context.Hakkimizda.CountAsync(),
                KayitFormuCount = await _context.KayitFormu.CountAsync(),
                IletisimCount = await _context.Iletisim.CountAsync(),
                BizeYazinCount = await _context.BizeYazin.CountAsync(),
                GaleriCount = await _context.Gallery.CountAsync(),
                ActiveLayout = activeLayout
            };
            
            return View(adminDashboard);
        }
        
        // GET: /Admin/Login
        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        // POST: /Admin/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                // Here you would validate the credentials against your auth system
                // For demo purposes, let's use a simple hardcoded check
                // In a real app, this would use proper authentication
                if (model.Username == "admin" && model.Password == "admin123")
                {
                    // Create claims for the authenticated user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, "Admin"),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                        });

                    // Redirect to returnUrl if specified, otherwise to admin home
                    return RedirectToLocal(returnUrl);
                }
                
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            }
            
            return View(model);
        }
        
        // POST: /Admin/Logout
        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }
        
        // GET: /Admin/LayoutSettings
        [HttpGet("LayoutSettings")]
        public async Task<IActionResult> LayoutSettings()
        {
            // Get current site settings or create a new one if none exists
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync() ?? new Models.SiteSettingsModel();
            
            // Create a view model for the layout settings
            var viewModel = new LayoutSettingsViewModel
            {
                ActiveLayout = siteSettings.ActiveLayout,
                CurrentLogoPath = siteSettings.ImagePath,
                AvailableLayouts = new List<SelectListItem>
                {
                    new SelectListItem { Value = "_Layout", Text = "Default Layout", Selected = siteSettings.ActiveLayout == "_Layout" },
                    new SelectListItem { Value = "_Layout1", Text = "Blue Layout", Selected = siteSettings.ActiveLayout == "_Layout1" },
                    new SelectListItem { Value = "_Layout2", Text = "Green Layout with Sidebar", Selected = siteSettings.ActiveLayout == "_Layout2" },
                    new SelectListItem { Value = "_Layout3", Text = "Dark Purple Layout", Selected = siteSettings.ActiveLayout == "_Layout3" }
                }
            };
            
            return View(viewModel);
        }
        
        // POST: /Admin/LayoutSettings
        [HttpPost("LayoutSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LayoutSettings(LayoutSettingsViewModel model)
        {
            try
            {
                // Get current site settings or create a new one if none exists
                var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
                
                if (siteSettings == null)
                {
                    // Create new settings if none exist
                    siteSettings = new Models.SiteSettingsModel
                    {
                        ActiveLayout = model.ActiveLayout,
                        CreationDate = DateTime.Now
                    };
                    _context.SiteSettings.Add(siteSettings);
                }
                
                // Handle logo upload if provided
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    // Delete old logo from Cloudinary if it exists
                    if (!string.IsNullOrEmpty(siteSettings.CloudinaryPublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(siteSettings.CloudinaryPublicId);
                    }
                    else if (!string.IsNullOrEmpty(siteSettings.ImagePath) && siteSettings.ImagePath.Contains("cloudinary"))
                    {
                        // Try to extract public ID from URL
                        var publicId = _cloudinaryService.GetPublicIdFromUrl(siteSettings.ImagePath);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                    }
                    
                    // Upload new logo to Cloudinary
                    var uploadResult = await _cloudinaryService.UploadImageAsync(model.Logo, "logo");
                    
                    if (uploadResult == null)
                    {
                        ModelState.AddModelError("Logo", "Logo yüklenemedi. Lütfen tekrar deneyin.");
                        return View(model);
                    }
                    
                    // Update the logo in the database
                    siteSettings.ImagePath = uploadResult.SecureUrl.ToString();
                    siteSettings.CloudinaryPublicId = uploadResult.PublicId;
                }
                
                // Update existing settings
                siteSettings.ActiveLayout = model.ActiveLayout;
                
                await _context.SaveChangesAsync();
                
                // Add success message
                TempData["SuccessMessage"] = $"Site ayarları başarıyla güncellendi.";
                
                return RedirectToAction(nameof(LayoutSettings));
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError(string.Empty, $"Hata oluştu: {ex.Message}");
            }
            
            // Repopulate the view model
            model.AvailableLayouts = new List<SelectListItem>
            {
                new SelectListItem { Value = "_Layout", Text = "Default Layout", Selected = model.ActiveLayout == "_Layout" },
                new SelectListItem { Value = "_Layout1", Text = "Blue Layout", Selected = model.ActiveLayout == "_Layout1" },
                new SelectListItem { Value = "_Layout2", Text = "Green Layout with Sidebar", Selected = model.ActiveLayout == "_Layout2" },
                new SelectListItem { Value = "_Layout3", Text = "Dark Purple Layout", Selected = model.ActiveLayout == "_Layout3" }
            };
            
            return View(model);
        }
    }
    
    // View models for the admin dashboard
    public class AdminDashboardViewModel
    {
        public int BannerCount { get; set; }
        public int IcerikCount { get; set; }
        public int KurslarCount { get; set; }
        public int DuyurularCount { get; set; }
        public int KadromuzCount { get; set; }
        public int SubelerCount { get; set; }
        public int HakkimizdaCount { get; set; }
        public int KayitFormuCount { get; set; }
        public int IletisimCount { get; set; }
        public int BizeYazinCount { get; set; }
        public string ActiveLayout { get; set; } = "_Layout";
        public int GaleriCount { get; set; }
    }
    
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    
    public class LayoutSettingsViewModel
    {
        [Required(ErrorMessage = "Aktif düzen seçilmelidir")]
        [Display(Name = "Aktif Düzen")]
        public string ActiveLayout { get; set; } = "_Layout";
        
        [Display(Name = "Site Logosu")]
        public IFormFile Logo { get; set; }
        
        public string CurrentLogoPath { get; set; }
        
        public List<SelectListItem> AvailableLayouts { get; set; } = new List<SelectListItem>();
    }
} 
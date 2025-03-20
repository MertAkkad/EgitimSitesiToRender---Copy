using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Services;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Banner")]
    public class BannerAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CloudinaryService _cloudinaryService;

        public BannerAdminController(
            ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment,
            CloudinaryService cloudinaryService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners.OrderBy(b => b.Order).ToListAsync();
            return View("~/Views/Admin/Banner/Index.cshtml", banners);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Get the next available order number
            int nextOrder = 1;
            if (_context.Banners.Any())
            {
                nextOrder = _context.Banners.Max(b => b.Order) + 1;
            }

            var banner = new BannerModel { Order = nextOrder, IsActive = true };
            return View("~/Views/Admin/Banner/Create.cshtml", banner);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BannerModel banner, IFormFile imageFile)
        {
            // For new banners, image is required
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Lütfen resim yükleyiniz");
                return View("~/Views/Admin/Banner/Create.cshtml", banner);
            }

            // Manually check required fields instead of relying on ModelState.IsValid
            if (string.IsNullOrEmpty(banner.Title))
            {
                ModelState.AddModelError("Title", "Başlık alanı zorunludur");
                return View("~/Views/Admin/Banner/Create.cshtml", banner);
            }

            // Continue even if ModelState is not valid for other fields
            try
            {
                // Upload image to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "banners");
                
                if (uploadResult == null)
                {
                    ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                    return View("~/Views/Admin/Banner/Create.cshtml", banner);
                }
                
                // Update the banner with cloudinary URL and public ID
                banner.ImagePath = uploadResult.SecureUrl.ToString();
                banner.CloudinaryPublicId = uploadResult.PublicId;
                banner.CreationDate = DateTime.Now;

                // Shift order of other banners if necessary
                if (banner.Order > 0)
                {
                    await ShiftBannerOrders(banner.Order);
                }
                else
                {
                    // Set order to the end if not specified
                    var maxOrder = await _context.Banners.MaxAsync(b => (int?)b.Order) ?? 0;
                    banner.Order = maxOrder + 1;
                }

                _context.Add(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                return View("~/Views/Admin/Banner/Create.cshtml", banner);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Banner/Edit.cshtml", banner);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BannerModel banner, IFormFile imageFile)
        {
            if (id != banner.Id)
            {
                return NotFound();
            }

            // Get the existing banner to compare
            var existingBanner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (existingBanner == null)
            {
                return NotFound();
            }

            // Continue with the update logic
            try
            {
                // Handle image upload if a new image is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete the old image from Cloudinary if it has a public ID
                    if (!string.IsNullOrEmpty(existingBanner.CloudinaryPublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(existingBanner.CloudinaryPublicId);
                    }
                    else if (!string.IsNullOrEmpty(existingBanner.ImagePath) && existingBanner.ImagePath.Contains("cloudinary"))
                    {
                        // Try to extract public ID from URL
                        var publicId = _cloudinaryService.GetPublicIdFromUrl(existingBanner.ImagePath);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                    }

                    // Upload new image
                    var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "banners");
                    if (uploadResult == null)
                    {
                        ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                        return View("~/Views/Admin/Banner/Edit.cshtml", banner);
                    }
                    
                    banner.ImagePath = uploadResult.SecureUrl.ToString();
                    banner.CloudinaryPublicId = uploadResult.PublicId;
                }
                else
                {
                    // Keep the existing image and cloudinary public ID
                    banner.ImagePath = existingBanner.ImagePath;
                    banner.CloudinaryPublicId = existingBanner.CloudinaryPublicId;
                }

                // Handle order changes if necessary
                if (banner.Order != existingBanner.Order)
                {
                    await ShiftBannerOrders(banner.Order, banner.Id);
                }

                // Preserve creation date
                banner.CreationDate = existingBanner.CreationDate;

                _context.Update(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BannerExists(banner.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                return View("~/Views/Admin/Banner/Edit.cshtml", banner);
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners.FirstOrDefaultAsync(m => m.Id == id);
            if (banner == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Banner/Delete.cshtml", banner);
        }

        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            // Delete image from Cloudinary if it has a public ID
            if (!string.IsNullOrEmpty(banner.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(banner.CloudinaryPublicId);
            }
            else if (!string.IsNullOrEmpty(banner.ImagePath) && banner.ImagePath.Contains("cloudinary"))
            {
                // Try to extract public ID from URL
                var publicId = _cloudinaryService.GetPublicIdFromUrl(banner.ImagePath);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.Id == id);
        }

        private async Task ShiftBannerOrders(int newOrder, int? excludeId = null)
        {
            // Get all banners with order >= newOrder, except the one being updated
            var bannersToShift = await _context.Banners
                .Where(b => b.Order >= newOrder && (excludeId == null || b.Id != excludeId))
                .OrderBy(b => b.Order)
                .ToListAsync();

            // Shift each banner's order up by 1
            foreach (var banner in bannersToShift)
            {
                banner.Order++;
                _context.Update(banner);
            }

            // Save the changes if any banners were updated
            if (bannersToShift.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // New action for handling banner reordering
        [HttpPost("ReorderBanners")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderBanners([FromBody] List<int> bannerIds)
        {
            if (bannerIds == null || bannerIds.Count == 0)
            {
                return BadRequest("Banner listesi boş olamaz.");
            }

            try
            {
                // Get all banners involved in the reordering
                var banners = await _context.Banners
                    .Where(b => bannerIds.Contains(b.Id))
                    .ToListAsync();

                // Update the order of each banner
                for (int i = 0; i < bannerIds.Count; i++)
                {
                    var bannerId = bannerIds[i];
                    var banner = banners.FirstOrDefault(b => b.Id == bannerId);
                    
                    if (banner != null)
                    {
                        banner.Order = i + 1; // New order starts at 1
                        _context.Update(banner);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Banner sıralaması güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 
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

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Banner")]
    public class BannerAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BannerAdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
            return View("~/Views/Admin/Banner/Create.cshtml");
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
                // Handle file upload
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "banners");
                Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                banner.ImagePath = "/uploads/banners/" + uniqueFileName;

                // Set order if not specified
                if (banner.Order <= 0)
                {
                    var maxOrder = await _context.Banners.MaxAsync(b => (int?)b.Order) ?? 0;
                    banner.Order = maxOrder + 1;
                }
                else
                {
                    // Shift other banners if this order is already taken
                    await ShiftBannerOrders(banner.Order);
                }

                // Set creation date
                banner.CreationDate = DateTime.Now;

                // Add to database
                _context.Add(banner);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
            }

            return View("~/Views/Admin/Banner/Create.cshtml", banner);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
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

            // Manually check required fields instead of relying on ModelState.IsValid
            if (string.IsNullOrEmpty(banner.Title))
            {
                ModelState.AddModelError("Title", "Başlık alanı zorunludur");
                return View("~/Views/Admin/Banner/Edit.cshtml", banner);
            }

            if (banner.Order <= 0 || banner.Order > 100)
            {
                ModelState.AddModelError("Order", "Sıralama 1-100 arasında olmalıdır");
                return View("~/Views/Admin/Banner/Edit.cshtml", banner);
            }

            // Continue even if ModelState is not valid for other fields
            try
            {
                // Get the existing banner to check for changes
                var existingBanner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                if (existingBanner == null)
                {
                    return NotFound();
                }

                // Handle file upload only if a new image is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingBanner.ImagePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBanner.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Upload new image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "banners");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    banner.ImagePath = "/uploads/banners/" + uniqueFileName;
                }
                else
                {
                    // Keep the existing image path
                    banner.ImagePath = existingBanner.ImagePath;
                }

                // Check if order has changed
                if (banner.Order != existingBanner.Order)
                {
                    await ShiftBannerOrders(banner.Order, banner.Id);
                }

                // Preserve creation date
                banner.CreationDate = existingBanner.CreationDate;

                // Update the entity
                _context.Update(banner);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BannerExists(banner.Id))
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
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
            }

            return View("~/Views/Admin/Banner/Edit.cshtml", banner);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
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

            // Delete the image file
            if (!string.IsNullOrEmpty(banner.ImagePath))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, banner.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Remove from database
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();

            // Reorder remaining banners
            await ReorderBannersAfterDelete();

            return RedirectToAction(nameof(Index));
        }

        private async Task ShiftBannerOrders(int newOrder, int? excludeId = null)
        {
            var bannersToUpdate = await _context.Banners
                .Where(b => b.Order >= newOrder && (excludeId == null || b.Id != excludeId))
                .OrderBy(b => b.Order)
                .ToListAsync();

            foreach (var banner in bannersToUpdate)
            {
                banner.Order++;
                _context.Update(banner);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ReorderBannersAfterDelete()
        {
            var banners = await _context.Banners.OrderBy(b => b.Order).ToListAsync();
            for (int i = 0; i < banners.Count; i++)
            {
                if (banners[i].Order != i + 1)
                {
                    banners[i].Order = i + 1;
                    _context.Update(banners[i]);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<bool> BannerExists(int id)
        {
            return await _context.Banners.AnyAsync(e => e.Id == id);
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
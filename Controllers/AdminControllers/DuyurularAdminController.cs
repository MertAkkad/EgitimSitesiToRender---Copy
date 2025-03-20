using Microsoft.AspNetCore.Mvc;
using EgitimSitesi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using EgitimSitesi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using EgitimSitesi.Services;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Duyurular")]
    public class DuyurularAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CloudinaryService _cloudinaryService;

        public DuyurularAdminController(
            ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment,
            CloudinaryService cloudinaryService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/DuyuruYonetimi
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var duyurular = await _context.Duyurular.OrderBy(d => d.Order).ToListAsync();
            return View("~/Views/Admin/Duyurular/Index.cshtml", duyurular);
        }

        // GET: Admin/DuyuruYonetimi/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Duyurular/Details.cshtml", duyuru);
        }

        // GET: Admin/DuyuruYonetimi/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Duyurular/Create.cshtml");
        }

        // POST: Admin/DuyuruYonetimi/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DuyurularModel duyuru, IFormFile imageFile)
        {
            try
            {
                // Handle image upload if present
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Upload image to Cloudinary
                    var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "duyurular");
                    
                    if (uploadResult == null)
                    {
                        ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                        return View("~/Views/Admin/Duyurular/Create.cshtml", duyuru);
                    }
                    
                    // Update the announcement with cloudinary URL and public ID
                    duyuru.ImagePath = uploadResult.SecureUrl.ToString();
                    duyuru.CloudinaryPublicId = uploadResult.PublicId;
                }

                // Set order if not specified
                if (duyuru.Order <= 0)
                {
                    var maxOrder = await _context.Duyurular.MaxAsync(d => (int?)d.Order) ?? 0;
                    duyuru.Order = maxOrder + 1;
                }
                else
                {
                    // Shift other announcements if this order is already taken
                    await ShiftDuyuruOrders(duyuru.Order);
                }

                // Set creation date
                duyuru.CreationDate = DateTime.Now;
                
                // Add to database
                _context.Add(duyuru);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                return View("~/Views/Admin/Duyurular/Create.cshtml", duyuru);
            }
        }

        // GET: Admin/DuyuruYonetimi/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Duyurular/Edit.cshtml", duyuru);
        }

        // POST: Admin/DuyuruYonetimi/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DuyurularModel duyuru, IFormFile imageFile)
        {
            if (id != duyuru.Id)
            {
                return NotFound();
            }

            // Get the existing announcement to compare
            var existingDuyuru = await _context.Duyurular.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (existingDuyuru == null)
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
                    if (!string.IsNullOrEmpty(existingDuyuru.CloudinaryPublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(existingDuyuru.CloudinaryPublicId);
                    }
                    else if (!string.IsNullOrEmpty(existingDuyuru.ImagePath) && existingDuyuru.ImagePath.Contains("cloudinary"))
                    {
                        // Try to extract public ID from URL
                        var publicId = _cloudinaryService.GetPublicIdFromUrl(existingDuyuru.ImagePath);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                    }

                    // Upload new image
                    var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "duyurular");
                    if (uploadResult == null)
                    {
                        ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                        return View("~/Views/Admin/Duyurular/Edit.cshtml", duyuru);
                    }
                    
                    duyuru.ImagePath = uploadResult.SecureUrl.ToString();
                    duyuru.CloudinaryPublicId = uploadResult.PublicId;
                }
                else
                {
                    // Keep the existing image and cloudinary public ID
                    duyuru.ImagePath = existingDuyuru.ImagePath;
                    duyuru.CloudinaryPublicId = existingDuyuru.CloudinaryPublicId;
                }

                // Check if order has changed
                if (duyuru.Order != existingDuyuru.Order)
                {
                    await ShiftDuyuruOrders(duyuru.Order, duyuru.Id);
                }

                // Preserve creation date
                duyuru.CreationDate = existingDuyuru.CreationDate;

                _context.Update(duyuru);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                return View("~/Views/Admin/Duyurular/Edit.cshtml", duyuru);
            }
        }

        // GET: Admin/DuyuruYonetimi/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Duyurular/Delete.cshtml", duyuru);
        }

        // POST: Admin/DuyuruYonetimi/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
            {
                return NotFound();
            }

            // Delete image from Cloudinary if it has a public ID
            if (!string.IsNullOrEmpty(duyuru.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(duyuru.CloudinaryPublicId);
            }
            else if (!string.IsNullOrEmpty(duyuru.ImagePath) && duyuru.ImagePath.Contains("cloudinary"))
            {
                // Try to extract public ID from URL
                var publicId = _cloudinaryService.GetPublicIdFromUrl(duyuru.ImagePath);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
            }

            _context.Duyurular.Remove(duyuru);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task ShiftDuyuruOrders(int newOrder, int? excludeId = null)
        {
            // Get all announcements with order >= newOrder, except the one being updated
            var duyurularToShift = await _context.Duyurular
                .Where(d => d.Order >= newOrder && (excludeId == null || d.Id != excludeId))
                .OrderBy(d => d.Order)
                .ToListAsync();

            // Shift each announcement's order up by 1
            foreach (var duyuru in duyurularToShift)
            {
                duyuru.Order++;
                _context.Update(duyuru);
            }

            // Save the changes if any announcements were updated
            if (duyurularToShift.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
} 
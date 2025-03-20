using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using EgitimSitesi.Services;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Kurslar")]
    public class KursAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly CloudinaryService _cloudinaryService;

        public KursAdminController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            CloudinaryService cloudinaryService)
        {
            _context = context;
            _environment = environment;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/Kurs
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kurslar = await _context.Kurslar.OrderBy(k => k.Order).ToListAsync();
            return View("~/Views/Admin/Kurslar/Index.cshtml", kurslar);
        }

        // GET: Admin/Kurs/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Kurslar/Create.cshtml");
        }

        // POST: Admin/Kurs/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KursModel kurs, IFormFile imageFile)
        {
         
                try
                {
                    // Set the highest order + 1 for new courses
                    if (_context.Kurslar.Any())
                    {
                        kurs.Order = _context.Kurslar.Max(k => k.Order) + 1;
                    }
                    else
                    {
                        kurs.Order = 1;
                    }

                    // Upload image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Upload to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "kurslar");
                        
                        if (uploadResult == null)
                        {
                            ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                            return View("~/Views/Admin/Kurslar/Create.cshtml", kurs);
                        }
                        
                        // Update the model with cloudinary URL and public ID
                        kurs.ImagePath = uploadResult.SecureUrl.ToString();
                        kurs.CloudinaryPublicId = uploadResult.PublicId;
                    }

                    // Set creation date
                    kurs.CreationDate = DateTime.Now;

                    // Add to database
                    _context.Add(kurs);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                }
            

            return View("~/Views/Admin/Kurslar/Create.cshtml", kurs);
        }

        // GET: Admin/Kurs/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurs = await _context.Kurslar.FindAsync(id);
            if (kurs == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Kurslar/Edit.cshtml", kurs);
        }

        // POST: Admin/Kurs/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KursModel kurs, IFormFile imageFile)
        {
            if (id != kurs.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKurs = await _context.Kurslar.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id);
                    if (existingKurs == null)
                    {
                        return NotFound();
                    }

                    // Process image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image from Cloudinary if it exists
                        if (!string.IsNullOrEmpty(existingKurs.CloudinaryPublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(existingKurs.CloudinaryPublicId);
                        }
                        else if (!string.IsNullOrEmpty(existingKurs.ImagePath) && existingKurs.ImagePath.Contains("cloudinary"))
                        {
                            // Try to extract public ID from URL
                            var publicId = _cloudinaryService.GetPublicIdFromUrl(existingKurs.ImagePath);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }

                        // Upload to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "kurslar");
                        
                        if (uploadResult == null)
                        {
                            ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                            return View("~/Views/Admin/Kurslar/Edit.cshtml", kurs);
                        }
                        
                        // Update model with Cloudinary information
                        kurs.ImagePath = uploadResult.SecureUrl.ToString();
                        kurs.CloudinaryPublicId = uploadResult.PublicId;
                    }
                    else
                    {
                        // Keep existing image and public ID
                        kurs.ImagePath = existingKurs.ImagePath;
                        kurs.CloudinaryPublicId = existingKurs.CloudinaryPublicId;
                    }

                    // Preserve creation date and order
                    kurs.CreationDate = existingKurs.CreationDate;
                    kurs.Order = existingKurs.Order;

                    _context.Update(kurs);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KursExists(kurs.Id))
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
                }
            }

            return View("~/Views/Admin/Kurslar/Edit.cshtml", kurs);
        }

        // GET: Admin/Kurs/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurs = await _context.Kurslar.FirstOrDefaultAsync(m => m.Id == id);
            if (kurs == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Kurslar/Delete.cshtml", kurs);
        }

        // POST: Admin/Kurs/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kurs = await _context.Kurslar.FindAsync(id);
            if (kurs == null)
            {
                return NotFound();
            }

            // Delete image from Cloudinary if it has a public ID
            if (!string.IsNullOrEmpty(kurs.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(kurs.CloudinaryPublicId);
            }
            else if (!string.IsNullOrEmpty(kurs.ImagePath) && kurs.ImagePath.Contains("cloudinary"))
            {
                // Try to extract public ID from URL
                var publicId = _cloudinaryService.GetPublicIdFromUrl(kurs.ImagePath);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
            }

            _context.Kurslar.Remove(kurs);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Move Up: Admin/Kurs/MoveUp/5
        [HttpPost("MoveUp/{id}")]
        public async Task<IActionResult> MoveUp(int id)
        {
            var currentKurs = await _context.Kurslar.FindAsync(id);
            if (currentKurs == null)
            {
                return NotFound();
            }

            var higherOrderKurs = await _context.Kurslar
                .Where(k => k.Order < currentKurs.Order)
                .OrderByDescending(k => k.Order)
                .FirstOrDefaultAsync();

            if (higherOrderKurs != null)
            {
                // Swap orders
                var tempOrder = currentKurs.Order;
                currentKurs.Order = higherOrderKurs.Order;
                higherOrderKurs.Order = tempOrder;

                _context.Update(currentKurs);
                _context.Update(higherOrderKurs);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Move Down: Admin/Kurs/MoveDown/5
        [HttpPost("MoveDown/{id}")]
        public async Task<IActionResult> MoveDown(int id)
        {
            var currentKurs = await _context.Kurslar.FindAsync(id);
            if (currentKurs == null)
            {
                return NotFound();
            }

            var lowerOrderKurs = await _context.Kurslar
                .Where(k => k.Order > currentKurs.Order)
                .OrderBy(k => k.Order)
                .FirstOrDefaultAsync();

            if (lowerOrderKurs != null)
            {
                // Swap orders
                var tempOrder = currentKurs.Order;
                currentKurs.Order = lowerOrderKurs.Order;
                lowerOrderKurs.Order = tempOrder;

                _context.Update(currentKurs);
                _context.Update(lowerOrderKurs);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KursExists(int id)
        {
            return _context.Kurslar.Any(e => e.Id == id);
        }
    }
} 
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using EgitimSitesi.Services;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Hakkimizda")]
    public class HakkimizdaAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly CloudinaryService _cloudinaryService;

        public HakkimizdaAdminController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            CloudinaryService cloudinaryService)
        {
            _context = context;
            _environment = environment;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/Hakkimizda
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var hakkimizda = await _context.Hakkimizda.ToListAsync();
            return View("~/Views/Admin/Hakkimizda/Index.cshtml", hakkimizda);
        }

        // GET: Admin/Hakkimizda/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hakkimizda = await _context.Hakkimizda
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (hakkimizda == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Hakkimizda/Details.cshtml", hakkimizda);
        }

        // GET: Admin/Hakkimizda/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Hakkimizda/Create.cshtml");
        }

        // POST: Admin/Hakkimizda/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HakkimizdaModel hakkimizda, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Upload to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "hakkimizda");
                        
                        if (uploadResult == null)
                        {
                            ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                            return View("~/Views/Admin/Hakkimizda/Create.cshtml", hakkimizda);
                        }
                        
                        // Update the model with cloudinary URL and public ID
                        hakkimizda.ImagePath = uploadResult.SecureUrl.ToString();
                        hakkimizda.CloudinaryPublicId = uploadResult.PublicId;
                    }

                    // Set creation date
                    hakkimizda.CreationDate = DateTime.Now;

                    // Add to database
                    _context.Add(hakkimizda);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                }
            }

            return View("~/Views/Admin/Hakkimizda/Create.cshtml", hakkimizda);
        }

        // GET: Admin/Hakkimizda/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hakkimizda = await _context.Hakkimizda.FindAsync(id);
            if (hakkimizda == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Hakkimizda/Edit.cshtml", hakkimizda);
        }

        // POST: Admin/Hakkimizda/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HakkimizdaModel hakkimizda, IFormFile imageFile)
        {
            if (id != hakkimizda.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingHakkimizda = await _context.Hakkimizda.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
                    if (existingHakkimizda == null)
                    {
                        return NotFound();
                    }

                    // Process image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image from Cloudinary if it exists
                        if (!string.IsNullOrEmpty(existingHakkimizda.CloudinaryPublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(existingHakkimizda.CloudinaryPublicId);
                        }
                        else if (!string.IsNullOrEmpty(existingHakkimizda.ImagePath) && existingHakkimizda.ImagePath.Contains("cloudinary"))
                        {
                            // Try to extract public ID from URL
                            var publicId = _cloudinaryService.GetPublicIdFromUrl(existingHakkimizda.ImagePath);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }

                        // Upload to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "hakkimizda");
                        
                        if (uploadResult == null)
                        {
                            ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                            return View("~/Views/Admin/Hakkimizda/Edit.cshtml", hakkimizda);
                        }
                        
                        // Update model with Cloudinary information
                        hakkimizda.ImagePath = uploadResult.SecureUrl.ToString();
                        hakkimizda.CloudinaryPublicId = uploadResult.PublicId;
                    }
                    else
                    {
                        // Keep existing image and public ID
                        hakkimizda.ImagePath = existingHakkimizda.ImagePath;
                        hakkimizda.CloudinaryPublicId = existingHakkimizda.CloudinaryPublicId;
                    }

                    // Preserve creation date
                    hakkimizda.CreationDate = existingHakkimizda.CreationDate;

                    _context.Update(hakkimizda);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HakkimizdaExists(hakkimizda.Id))
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

            return View("~/Views/Admin/Hakkimizda/Edit.cshtml", hakkimizda);
        }

        // GET: Admin/Hakkimizda/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hakkimizda = await _context.Hakkimizda.FindAsync(id);
            if (hakkimizda == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Hakkimizda/Delete.cshtml", hakkimizda);
        }

        // POST: Admin/Hakkimizda/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hakkimizda = await _context.Hakkimizda.FindAsync(id);
            if (hakkimizda == null)
            {
                return NotFound();
            }

            // Delete image from Cloudinary if it has a public ID
            if (!string.IsNullOrEmpty(hakkimizda.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(hakkimizda.CloudinaryPublicId);
            }
            else if (!string.IsNullOrEmpty(hakkimizda.ImagePath) && hakkimizda.ImagePath.Contains("cloudinary"))
            {
                // Try to extract public ID from URL
                var publicId = _cloudinaryService.GetPublicIdFromUrl(hakkimizda.ImagePath);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
            }

            _context.Hakkimizda.Remove(hakkimizda);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Hakkimizda/Toggle/5
        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var hakkimizda = await _context.Hakkimizda.FindAsync(id);
            if (hakkimizda == null)
            {
                return NotFound();
            }

            // Toggle active status
            hakkimizda.IsActive = !hakkimizda.IsActive;
            
            _context.Update(hakkimizda);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool HakkimizdaExists(int id)
        {
            return _context.Hakkimizda.Any(e => e.Id == id);
        }
    }
} 
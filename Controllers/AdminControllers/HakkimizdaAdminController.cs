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

        public HakkimizdaAdminController(ApplicationDbContext context, IWebHostEnvironment environment, CloudinaryService cloudinaryService)
        {
            _context = context;
            _environment = environment;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/Hakkimizda
        [HttpGet("")]
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
            // Check if a record already exists since this should be a singleton
            if (_context.Hakkimizda.Any())
            {
                return RedirectToAction(nameof(Index));
            }
            
            return View("~/Views/Admin/Hakkimizda/Create.cshtml");
        }

        // POST: Admin/Hakkimizda/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HakkimizdaModel hakkimizda, IFormFile imageFile)
        {
           
                try
                {
                    // Process image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Upload to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "hakkimizda");
                        if (uploadResult != null)
                        {
                            hakkimizda.ImagePath = uploadResult.SecureUrl.ToString();
                            hakkimizda.CloudinaryPublicId = uploadResult.PublicId;
                        }
                        else
                        {
                            // Fallback to local storage if Cloudinary upload fails
                            // Create uploads directory if it doesn't exist
                            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "hakkimizda");
                            if (!Directory.Exists(uploadsDir))
                            {
                                Directory.CreateDirectory(uploadsDir);
                            }

                            // Create unique filename
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                            var filePath = Path.Combine(uploadsDir, uniqueFileName);

                            // Save the file
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            // Set the image path
                            hakkimizda.ImagePath = "/uploads/hakkimizda/" + uniqueFileName;
                        }
                    }

                    // Set creation date
                    hakkimizda.CreationDate = DateTime.Now;
                    
                    // Set a default empty value for CloudinaryPublicId to prevent NULL constraint violation
                    if (string.IsNullOrEmpty(hakkimizda.CloudinaryPublicId))
                    {
                        hakkimizda.CloudinaryPublicId = "";
                    }

                    // Add to database
                    _context.Add(hakkimizda);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
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
                        // Delete the existing image from Cloudinary if it exists
                        if (!string.IsNullOrEmpty(existingHakkimizda.CloudinaryPublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(existingHakkimizda.CloudinaryPublicId);
                        }
                        else if (!string.IsNullOrEmpty(existingHakkimizda.ImagePath) && existingHakkimizda.ImagePath.Contains("cloudinary"))
                        {
                            // Extract public ID from URL
                            var publicId = _cloudinaryService.GetPublicIdFromUrl(existingHakkimizda.ImagePath);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }

                        // Upload to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "hakkimizda");
                        if (uploadResult != null)
                        {
                            hakkimizda.ImagePath = uploadResult.SecureUrl.ToString();
                            hakkimizda.CloudinaryPublicId = uploadResult.PublicId;
                        }
                        else
                        {
                            // Delete the existing local image if there is one
                            if (!string.IsNullOrEmpty(existingHakkimizda.ImagePath) && !existingHakkimizda.ImagePath.Contains("cloudinary"))
                            {
                                var oldFilePath = Path.Combine(_environment.WebRootPath, existingHakkimizda.ImagePath.TrimStart('/'));
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                            }

                            // Create uploads directory if it doesn't exist
                            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "hakkimizda");
                            if (!Directory.Exists(uploadsDir))
                            {
                                Directory.CreateDirectory(uploadsDir);
                            }

                            // Create unique filename
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                            var filePath = Path.Combine(uploadsDir, uniqueFileName);

                            // Save the file
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            // Set the new image path
                            hakkimizda.ImagePath = "/uploads/hakkimizda/" + uniqueFileName;
                        }
                    }
                    else
                    {
                        // Keep the existing image path and CloudinaryPublicId
                        hakkimizda.ImagePath = existingHakkimizda.ImagePath;
                        hakkimizda.CloudinaryPublicId = existingHakkimizda.CloudinaryPublicId;
                    }

                    // Preserve the creation date
                    hakkimizda.CreationDate = existingHakkimizda.CreationDate;
                    
                    // Ensure CloudinaryPublicId is not null
                    if (string.IsNullOrEmpty(hakkimizda.CloudinaryPublicId))
                    {
                        hakkimizda.CloudinaryPublicId = "";
                    }

                    // Update the entity
                    _context.Update(hakkimizda);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await HakkimizdaExists(hakkimizda.Id))
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

            var hakkimizda = await _context.Hakkimizda
                .FirstOrDefaultAsync(m => m.Id == id);
                
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
            if (hakkimizda != null)
            {
                // Delete image from Cloudinary if it exists
                if (!string.IsNullOrEmpty(hakkimizda.CloudinaryPublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(hakkimizda.CloudinaryPublicId);
                }
                else if (!string.IsNullOrEmpty(hakkimizda.ImagePath) && hakkimizda.ImagePath.Contains("cloudinary"))
                {
                    // Extract public ID from URL
                    var publicId = _cloudinaryService.GetPublicIdFromUrl(hakkimizda.ImagePath);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }

                // Delete local file if it exists
                if (!string.IsNullOrEmpty(hakkimizda.ImagePath) && !hakkimizda.ImagePath.Contains("cloudinary"))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, hakkimizda.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Hakkimizda.Remove(hakkimizda);
                await _context.SaveChangesAsync();
            }
            
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

        private async Task<bool> HakkimizdaExists(int id)
        {
            return await _context.Hakkimizda.AnyAsync(e => e.Id == id);
        }
    }
} 
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

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Hakkimizda")]
    public class HakkimizdaAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public HakkimizdaAdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
            // Check if a record already exists since this should be a singleton
            if (_context.Hakkimizda.Any())
            {
                ModelState.AddModelError("", "Hakkımızda bilgileri zaten mevcut. Yeni bir kayıt eklenemez.");
                return View("~/Views/Admin/Hakkimizda/Create.cshtml", hakkimizda);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Process image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
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

                    // Set creation date
                    hakkimizda.CreationDate = DateTime.Now;

                    // Add to database
                    _context.Add(hakkimizda);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
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
                        // Delete the existing image if there is one
                        if (!string.IsNullOrEmpty(existingHakkimizda.ImagePath))
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
                    else
                    {
                        // Keep the existing image path
                        hakkimizda.ImagePath = existingHakkimizda.ImagePath;
                    }

                    // Preserve the creation date
                    hakkimizda.CreationDate = existingHakkimizda.CreationDate;

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
            
            // Delete image if it exists
            if (!string.IsNullOrEmpty(hakkimizda.ImagePath))
            {
                var filePath = Path.Combine(_environment.WebRootPath, hakkimizda.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
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

        private async Task<bool> HakkimizdaExists(int id)
        {
            return await _context.Hakkimizda.AnyAsync(e => e.Id == id);
        }
    }
} 
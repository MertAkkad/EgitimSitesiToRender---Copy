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

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Duyurular")]
    public class DuyurularAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DuyurularAdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
         
                // Handle image upload if present
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "duyurular");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    duyuru.ImagePath = "/uploads/duyurular/" + uniqueFileName;
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
                
                // Set announcement date if not specified
                if (duyuru.CreationDate == DateTime.MinValue)
                {
                    duyuru.CreationDate = DateTime.Now;
                }

                // Add to database
                _context.Add(duyuru);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            
            return View("~/Views/Admin/Duyurular/Create.cshtml", duyuru);
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

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing announcement to check for changes
                    var existingDuyuru = await _context.Duyurular.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    if (existingDuyuru == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload if a new image is provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingDuyuru.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingDuyuru.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Upload new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "duyurular");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        duyuru.ImagePath = "/uploads/duyurular/" + uniqueFileName;
                    }
                    else
                    {
                        // Keep existing image if no new one was uploaded
                        duyuru.ImagePath = existingDuyuru.ImagePath;
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DuyuruExists(duyuru.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/Duyurular/Edit.cshtml", duyuru);
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

            // Delete the image file if it exists
            if (!string.IsNullOrEmpty(duyuru.ImagePath))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, duyuru.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Duyurular.Remove(duyuru);
            await _context.SaveChangesAsync();

            // Reorder remaining announcements
            await ReorderDuyurularAfterDelete();

            return RedirectToAction(nameof(Index));
        }

        // Helpers for managing order
        private async Task ShiftDuyuruOrders(int newOrder, int? excludeId = null)
        {
            var duyurularToUpdate = await _context.Duyurular
                .Where(d => d.Order >= newOrder && (excludeId == null || d.Id != excludeId))
                .OrderBy(d => d.Order)
                .ToListAsync();

            foreach (var duyuru in duyurularToUpdate)
            {
                duyuru.Order++;
                _context.Update(duyuru);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ReorderDuyurularAfterDelete()
        {
            var duyurular = await _context.Duyurular.OrderBy(d => d.Order).ToListAsync();
            for (int i = 0; i < duyurular.Count; i++)
            {
                if (duyurular[i].Order != i + 1)
                {
                    duyurular[i].Order = i + 1;
                    _context.Update(duyurular[i]);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<bool> DuyuruExists(int id)
        {
            return await _context.Duyurular.AnyAsync(e => e.Id == id);
        }

        // AJAX endpoint for reordering via drag and drop
        [HttpPost("ReorderDuyurular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderDuyurular([FromBody] List<int> duyuruIds)
        {
            if (duyuruIds == null || duyuruIds.Count == 0)
            {
                return BadRequest("Duyuru listesi boş olamaz.");
            }

            try
            {
                // Get all announcements involved in the reordering
                var duyurular = await _context.Duyurular
                    .Where(d => duyuruIds.Contains(d.Id))
                    .ToListAsync();

                // Update the order of each announcement
                for (int i = 0; i < duyuruIds.Count; i++)
                {
                    var duyuruId = duyuruIds[i];
                    var duyuru = duyurular.FirstOrDefault(d => d.Id == duyuruId);
                    
                    if (duyuru != null)
                    {
                        duyuru.Order = i + 1; // New order starts at 1
                        _context.Update(duyuru);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Duyuru sıralaması güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 
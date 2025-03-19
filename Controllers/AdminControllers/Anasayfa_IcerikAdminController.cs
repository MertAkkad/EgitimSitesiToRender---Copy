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
    [Route("Admin/Icerik")]
    public class Anasayfa_IcerikAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Anasayfa_IcerikAdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/IcerikYonetimi
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var contentItems = await _context.AnasayfaIcerik.OrderBy(c => c.Order).ToListAsync();
            return View("~/Views/Admin/Icerik/Index.cshtml", contentItems);
        }

        // GET: Admin/IcerikYonetimi/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var contentItem = await _context.AnasayfaIcerik.FindAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Icerik/Details.cshtml", contentItem);
        }

        // GET: Admin/IcerikYonetimi/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Icerik/Create.cshtml");
        }

        // POST: Admin/IcerikYonetimi/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Anasayfa_IcerikModel contentItem, IFormFile imageFile)
        {
            // For new content items, image is required
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Lütfen resim yükleyiniz");
                return View("~/Views/Admin/Icerik/Create.cshtml", contentItem);
            }

            // Manually check required fields
            if (string.IsNullOrEmpty(contentItem.Title))
            {
                ModelState.AddModelError("Title", "Başlık alanı zorunludur");
                return View("~/Views/Admin/Icerik/Create.cshtml", contentItem);
            }

            if (string.IsNullOrEmpty(contentItem.Description))
            {
                ModelState.AddModelError("Description", "Açıklama alanı zorunludur");
                return View("~/Views/Admin/Icerik/Create.cshtml", contentItem);
            }

            if (string.IsNullOrEmpty(contentItem.ContentType))
            {
                ModelState.AddModelError("ContentType", "İçerik türü zorunludur");
                return View("~/Views/Admin/Icerik/Create.cshtml", contentItem);
            }

            try
            {
                // Handle file upload
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "icerikler");
                Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                contentItem.ImagePath = "/uploads/icerikler/" + uniqueFileName;

                // Set order if not specified
                if (contentItem.Order <= 0)
                {
                    var maxOrder = await _context.AnasayfaIcerik.MaxAsync(i => (int?)i.Order) ?? 0;
                    contentItem.Order = maxOrder + 1;
                }
                else
                {
                    // Shift other content items if this order is already taken
                    await ShiftIcerikOrders(contentItem.Order);
                }

                // Set creation date
                contentItem.CreationDate = DateTime.Now;

                // Add to database
                _context.Add(contentItem);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
            }

            return View("~/Views/Admin/Icerik/Create.cshtml", contentItem);
        }

        // GET: Admin/IcerikYonetimi/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var contentItem = await _context.AnasayfaIcerik.FindAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Icerik/Edit.cshtml", contentItem);
        }

        // POST: Admin/IcerikYonetimi/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Anasayfa_IcerikModel contentItem, IFormFile imageFile)
        {
            if (id != contentItem.Id)
            {
                return NotFound();
            }

            // Manually check required fields
            if (string.IsNullOrEmpty(contentItem.Title))
            {
                ModelState.AddModelError("Title", "Başlık alanı zorunludur");
                return View("~/Views/Admin/Icerik/Edit.cshtml", contentItem);
            }

            if (string.IsNullOrEmpty(contentItem.Description))
            {
                ModelState.AddModelError("Description", "Açıklama alanı zorunludur");
                return View("~/Views/Admin/Icerik/Edit.cshtml", contentItem);
            }

            if (string.IsNullOrEmpty(contentItem.ContentType))
            {
                ModelState.AddModelError("ContentType", "İçerik türü zorunludur");
                return View("~/Views/Admin/Icerik/Edit.cshtml", contentItem);
            }

            if (contentItem.Order <= 0 || contentItem.Order > 100)
            {
                ModelState.AddModelError("Order", "Sıralama 1-100 arasında olmalıdır");
                return View("~/Views/Admin/Icerik/Edit.cshtml", contentItem);
            }

            try
            {
                // Get the existing content item to check for changes
                var existingContentItem = await _context.AnasayfaIcerik.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                if (existingContentItem == null)
                {
                    return NotFound();
                }

                // Handle file upload only if a new image is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingContentItem.ImagePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingContentItem.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Upload new image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "icerikler");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    contentItem.ImagePath = "/uploads/icerikler/" + uniqueFileName;
                }
                else
                {
                    // Keep the existing image path
                    contentItem.ImagePath = existingContentItem.ImagePath;
                }

                // Check if order has changed
                if (contentItem.Order != existingContentItem.Order)
                {
                    await ShiftIcerikOrders(contentItem.Order, contentItem.Id);
                }

                // Preserve creation date
                contentItem.CreationDate = existingContentItem.CreationDate;

                // Update the entity
                _context.Update(contentItem);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await IcerikExists(contentItem.Id))
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

            return View("~/Views/Admin/Icerik/Edit.cshtml", contentItem);
        }

        // GET: Admin/IcerikYonetimi/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contentItem = await _context.AnasayfaIcerik.FindAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Icerik/Delete.cshtml", contentItem);
        }

        // POST: Admin/IcerikYonetimi/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contentItem = await _context.AnasayfaIcerik.FindAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            // Delete the image file
            if (!string.IsNullOrEmpty(contentItem.ImagePath))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, contentItem.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Remove from database
            _context.AnasayfaIcerik.Remove(contentItem);
            await _context.SaveChangesAsync();

            // Reorder remaining content items
            await ReorderIceriklerAfterDelete();

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/IcerikYonetimi/MoveUp/5
        [HttpPost("MoveUp/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var contentItem = await _context.AnasayfaIcerik.FindAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            // Can't move up if already at the top
            if (contentItem.Order <= 1)
            {
                return RedirectToAction(nameof(Index));
            }

            // Find the item above this one
            var aboveContentItem = await _context.AnasayfaIcerik
                .Where(i => i.Order == contentItem.Order - 1)
                .FirstOrDefaultAsync();

            if (aboveContentItem != null)
            {
                // Swap orders
                aboveContentItem.Order += 1;
                contentItem.Order -= 1;

                _context.Update(aboveContentItem);
                _context.Update(contentItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/IcerikYonetimi/MoveDown/5
        [HttpPost("MoveDown/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var contentItem = await _context.AnasayfaIcerik.FindAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            // Find the item below this one
            var belowContentItem = await _context.AnasayfaIcerik
                .Where(i => i.Order == contentItem.Order + 1)
                .FirstOrDefaultAsync();

            if (belowContentItem != null)
            {
                // Swap orders
                belowContentItem.Order -= 1;
                contentItem.Order += 1;

                _context.Update(belowContentItem);
                _context.Update(contentItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to shift orders when inserting or updating
        private async Task ShiftIcerikOrders(int newOrder, int? excludeId = null)
        {
            var contentItemsToUpdate = await _context.AnasayfaIcerik
                .Where(i => i.Order >= newOrder && (excludeId == null || i.Id != excludeId))
                .OrderBy(i => i.Order)
                .ToListAsync();

            foreach (var contentItem in contentItemsToUpdate)
            {
                contentItem.Order++;
                _context.Update(contentItem);
            }

            await _context.SaveChangesAsync();
        }

        // Helper method to reorder after deletion
        private async Task ReorderIceriklerAfterDelete()
        {
            var contentItems = await _context.AnasayfaIcerik.OrderBy(i => i.Order).ToListAsync();
            for (int i = 0; i < contentItems.Count; i++)
            {
                if (contentItems[i].Order != i + 1)
                {
                    contentItems[i].Order = i + 1;
                    _context.Update(contentItems[i]);
                }
            }
            await _context.SaveChangesAsync();
        }

        // Helper method to check if content item exists
        private async Task<bool> IcerikExists(int id)
        {
            return await _context.AnasayfaIcerik.AnyAsync(e => e.Id == id);
        }

        // AJAX endpoint for reordering via drag and drop
        [HttpPost("ReorderIcerikler")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderIcerikler([FromBody] List<int> contentItemIds)
        {
            if (contentItemIds == null || contentItemIds.Count == 0)
            {
                return BadRequest("İçerik listesi boş olamaz.");
            }

            try
            {
                // Get all content items involved in the reordering
                var contentItems = await _context.AnasayfaIcerik
                    .Where(i => contentItemIds.Contains(i.Id))
                    .ToListAsync();

                // Update the order of each content item
                for (int i = 0; i < contentItemIds.Count; i++)
                {
                    var contentItemId = contentItemIds[i];
                    var contentItem = contentItems.FirstOrDefault(i => i.Id == contentItemId);
                    
                    if (contentItem != null)
                    {
                        contentItem.Order = i + 1; // New order starts at 1
                        _context.Update(contentItem);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "İçerik sıralaması güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 
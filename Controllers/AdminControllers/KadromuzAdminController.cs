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
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using EgitimSitesi.Services;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Kadromuz")]
    public class KadromuzAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly CloudinaryService _cloudinaryService;

        public KadromuzAdminController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            CloudinaryService cloudinaryService)
        {
            _context = context;
            _environment = environment;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/Kadromuz
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var staff = await _context.Kadromuz.OrderBy(k => k.Order).ToListAsync();
            return View("~/Views/Admin/Kadromuz/Index.cshtml", staff);
        }

        // POST: Admin/Kadromuz/Reorder
        [HttpPost("Reorder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderKadromuz([FromBody] List<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
            {
                return BadRequest(new { success = false, message = "No IDs were provided." });
            }

            try
            {
                // Get all staff records that match the IDs
                var staffList = await _context.Kadromuz
                    .Where(k => itemIds.Contains(k.Id))
                    .ToListAsync();

                // Update order based on the position in the itemIds list
                foreach (var staff in staffList)
                {
                    int index = itemIds.IndexOf(staff.Id);
                    if (index != -1)
                    {
                        staff.Order = index + 1;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Kadromuz/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Kadromuz
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (staff == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Kadromuz/Details.cshtml", staff);
        }

        // GET: Admin/Kadromuz/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Get next order number
            int nextOrder = 1;
            if (_context.Kadromuz.Any())
            {
                nextOrder = _context.Kadromuz.Max(k => k.Order) + 1;
            }
            
            var staff = new KadromuzModel { Order = nextOrder, IsActive = true };
            return View("~/Views/Admin/Kadromuz/Create.cshtml", staff);
        }

        // POST: Admin/Kadromuz/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KadromuzModel staff, IFormFile imageFile)
        {
          
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Upload image to Cloudinary
                    var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "staff");
                    
                    if (uploadResult == null)
                    {
                        ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                        return View("~/Views/Admin/Kadromuz/Create.cshtml", staff);
                    }
                    
                    // Update the staff model with cloudinary URL and public ID
                    staff.ImagePath = uploadResult.SecureUrl.ToString();
                    staff.CloudinaryPublicId = uploadResult.PublicId;
                }

                staff.CreationDate = DateTime.Now;
                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           
            return View("~/Views/Admin/Kadromuz/Create.cshtml", staff);
        }

        // GET: Admin/Kadromuz/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Kadromuz.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Kadromuz/Edit.cshtml", staff);
        }

        // POST: Admin/Kadromuz/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KadromuzModel staff, IFormFile imageFile)
        {
            if (id != staff.Id)
            {
                return NotFound();
            }

          
                try
                {
                    var existingStaff = await _context.Kadromuz.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id);
                    if (existingStaff == null)
                    {
                        return NotFound();
                    }

                    // Handle Image (if provided)
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image from Cloudinary if it exists
                        if (!string.IsNullOrEmpty(existingStaff.CloudinaryPublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(existingStaff.CloudinaryPublicId);
                        }
                        else if (!string.IsNullOrEmpty(existingStaff.ImagePath) && existingStaff.ImagePath.Contains("cloudinary"))
                        {
                            // Try to extract public ID from URL
                            var publicId = _cloudinaryService.GetPublicIdFromUrl(existingStaff.ImagePath);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }
                        
                        // Upload new image to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "staff");
                        
                        if (uploadResult == null)
                        {
                            ModelState.AddModelError("imageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                            return View("~/Views/Admin/Kadromuz/Edit.cshtml", staff);
                        }
                        
                        staff.ImagePath = uploadResult.SecureUrl.ToString();
                        staff.CloudinaryPublicId = uploadResult.PublicId;
                    }
                    else
                    {
                        // Keep existing image and public ID
                        staff.ImagePath = existingStaff.ImagePath;
                        staff.CloudinaryPublicId = existingStaff.CloudinaryPublicId;
                    }

                    // Preserve creation date
                    staff.CreationDate = existingStaff.CreationDate;
                    
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(staff.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            
            return View("~/Views/Admin/Kadromuz/Edit.cshtml", staff);
        }

        // GET: Admin/Kadromuz/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Kadromuz.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Kadromuz/Delete.cshtml", staff);
        }

        // POST: Admin/Kadromuz/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Kadromuz.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            // Delete image from Cloudinary if it has a public ID
            if (!string.IsNullOrEmpty(staff.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(staff.CloudinaryPublicId);
            }
            else if (!string.IsNullOrEmpty(staff.ImagePath) && staff.ImagePath.Contains("cloudinary"))
            {
                // Try to extract public ID from URL
                var publicId = _cloudinaryService.GetPublicIdFromUrl(staff.ImagePath);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
            }

            _context.Kadromuz.Remove(staff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Kadromuz/MoveUp/5
        [HttpPost("MoveUp/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var staff = await _context.Kadromuz.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            
            // Find the item with the same title & grade and above this one in order
            var aboveStaff = await _context.Kadromuz
                .Where(k => k.Title == staff.Title && k.Grade == staff.Grade && k.Order < staff.Order)
                .OrderByDescending(k => k.Order)
                .FirstOrDefaultAsync();
                
            if (aboveStaff != null)
            {
                // Swap orders
                var tempOrder = staff.Order;
                staff.Order = aboveStaff.Order;
                aboveStaff.Order = tempOrder;
                
                _context.Update(staff);
                _context.Update(aboveStaff);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Kadromuz/MoveDown/5
        [HttpPost("MoveDown/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var staff = await _context.Kadromuz.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            
            // Find the item with the same title & grade and below this one in order
            var belowStaff = await _context.Kadromuz
                .Where(k => k.Title == staff.Title && k.Grade == staff.Grade && k.Order > staff.Order)
                .OrderBy(k => k.Order)
                .FirstOrDefaultAsync();
                
            if (belowStaff != null)
            {
                // Swap orders
                var tempOrder = staff.Order;
                staff.Order = belowStaff.Order;
                belowStaff.Order = tempOrder;
                
                _context.Update(staff);
                _context.Update(belowStaff);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
            return _context.Kadromuz.Any(e => e.Id == id);
        }
    }
} 
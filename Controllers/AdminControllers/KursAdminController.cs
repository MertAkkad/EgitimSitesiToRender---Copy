using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Kurslar")]
    public class KursAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public KursAdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Kurslar
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kurslar = await _context.Kurslar
                .OrderBy(k => k.Order)
                .ToListAsync();
            
            return View("~/Views/Admin/Kurslar/Index.cshtml", kurslar);
        }

        // GET: Admin/Kurslar/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Get the maximum order value
            int maxOrder = 0;
            if (_context.Kurslar.Any())
            {
                maxOrder = _context.Kurslar.Max(k => k.Order);
            }
            
            // Create a new model with default values
            var model = new KursModel
            {
                IsActive = true,
                Order = maxOrder + 1
            };
            
            return View("~/Views/Admin/Kurslar/Create.cshtml", model);
        }

        // POST: Admin/Kurslar/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KursModel model, IFormFile imageFile)
        {
           
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "kurslar");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    
                    model.ImagePath = "/uploads/kurslar/" + uniqueFileName;
                }
                
                model.CreationDate = DateTime.Now;
                
                _context.Add(model);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            
            
            return View("~/Views/Admin/Kurslar/Create.cshtml", model);
        }

        // GET: Admin/Kurslar/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var kurs = await _context.Kurslar.FindAsync(id);
            if (kurs == null)
            {
                return NotFound();
            }
            
            return View("~/Views/Admin/Kurslar/Edit.cshtml", kurs);
        }

        // POST: Admin/Kurslar/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KursModel model, IFormFile imageFile)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKurs = await _context.Kurslar.FindAsync(id);
                    if (existingKurs == null)
                    {
                        return NotFound();
                    }
                    
                    existingKurs.Type = model.Type;
                    existingKurs.Description = model.Description;
                    existingKurs.Details = model.Details;
                    existingKurs.Order = model.Order;
                    existingKurs.IsActive = model.IsActive;
                    
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete the old image file if it exists
                        if (!string.IsNullOrEmpty(existingKurs.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_environment.WebRootPath, existingKurs.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "kurslar");
                        Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
                        
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        
                        existingKurs.ImagePath = "/uploads/kurslar/" + uniqueFileName;
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KursExists(model.Id))
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
            
            return View("~/Views/Admin/Kurslar/Edit.cshtml", model);
        }

        // GET: Admin/Kurslar/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kurs = await _context.Kurslar.FindAsync(id);
            if (kurs == null)
            {
                return NotFound();
            }
            
            return View("~/Views/Admin/Kurslar/Delete.cshtml", kurs);
        }

        // POST: Admin/Kurslar/Delete/5
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kurs = await _context.Kurslar.FindAsync(id);
            if (kurs == null)
            {
                return NotFound();
            }
            
            // Delete the image file if it exists
            if (!string.IsNullOrEmpty(kurs.ImagePath))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, kurs.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            
            _context.Kurslar.Remove(kurs);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Kurslar/ReOrder
        [HttpPost("ReOrder")]
        public async Task<IActionResult> ReOrder([FromBody] ReOrderModel model)
        {
            if (model != null && model.Ids != null && model.Ids.Count > 0)
            {
                for (int i = 0; i < model.Ids.Count; i++)
                {
                    var kurs = await _context.Kurslar.FindAsync(model.Ids[i]);
                    if (kurs != null)
                    {
                        kurs.Order = i + 1;
                    }
                }
                
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            
            return Json(new { success = false, message = "No data received" });
        }

        private bool KursExists(int id)
        {
            return _context.Kurslar.Any(e => e.Id == id);
        }
    }
    
    public class ReOrderModel
    {
        public List<int> Ids { get; set; }
    }
} 
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using Microsoft.AspNetCore.Authorization;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Iletisim")]
    public class IletisimAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IletisimAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Iletisim
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var iletisim = await _context.Iletisim.ToListAsync();
            return View("~/Views/Admin/Iletisim/Index.cshtml", iletisim);
        }

        // GET: Admin/Iletisim/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iletisim = await _context.Iletisim
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (iletisim == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Iletisim/Details.cshtml", iletisim);
        }

        // GET: Admin/Iletisim/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Check if a record already exists (we generally only want one contact info record)
            if (_context.Iletisim.Any())
            {
                return RedirectToAction(nameof(Index));
            }
            
            return View("~/Views/Admin/Iletisim/Create.cshtml");
        }

        // POST: Admin/Iletisim/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IletisimModel iletisim)
        {
            // Check if a record already exists 
            if (_context.Iletisim.Any())
            {
                ModelState.AddModelError("", "İletişim bilgileri zaten mevcut. Yeni bir kayıt eklenemez.");
                return View("~/Views/Admin/Iletisim/Create.cshtml", iletisim);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set creation date
                    iletisim.CreationDate = DateTime.Now;
                    iletisim.IsActive = true;

                    // Add to database
                    _context.Add(iletisim);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                }
            }

            return View("~/Views/Admin/Iletisim/Create.cshtml", iletisim);
        }

        // GET: Admin/Iletisim/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iletisim = await _context.Iletisim.FindAsync(id);
            if (iletisim == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Iletisim/Edit.cshtml", iletisim);
        }

        // POST: Admin/Iletisim/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IletisimModel iletisim)
        {
            if (id != iletisim.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingIletisim = await _context.Iletisim.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
                    if (existingIletisim == null)
                    {
                        return NotFound();
                    }

                    // Preserve the creation date
                    iletisim.CreationDate = existingIletisim.CreationDate;

                    // Update the entity
                    _context.Update(iletisim);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await IletisimExists(iletisim.Id))
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

            return View("~/Views/Admin/Iletisim/Edit.cshtml", iletisim);
        }

        // GET: Admin/Iletisim/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iletisim = await _context.Iletisim
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (iletisim == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Iletisim/Delete.cshtml", iletisim);
        }

        // POST: Admin/Iletisim/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var iletisim = await _context.Iletisim.FindAsync(id);
            
            _context.Iletisim.Remove(iletisim);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Iletisim/Toggle/5
        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var iletisim = await _context.Iletisim.FindAsync(id);
            if (iletisim == null)
            {
                return NotFound();
            }

            // Toggle active status
            iletisim.IsActive = !iletisim.IsActive;
            
            _context.Update(iletisim);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> IletisimExists(int id)
        {
            return await _context.Iletisim.AnyAsync(e => e.Id == id);
        }
    }
} 
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
    [Route("Admin/KayitFormu")]
    public class KayitFormuAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KayitFormuAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/KayitFormu
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var registrations = await _context.KayitFormu
                .OrderByDescending(k => k.CreationDate)
                .ToListAsync();
                
            return View("~/Views/Admin/KayitFormu/Index.cshtml", registrations);
        }

        // GET: Admin/KayitFormu/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.KayitFormu
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (registration == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/KayitFormu/Details.cshtml", registration);
        }

        // POST: Admin/KayitFormu/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registration = await _context.KayitFormu.FindAsync(id);
            
            if (registration != null)
            {
                _context.KayitFormu.Remove(registration);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/KayitFormu/Toggle/5
        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var registration = await _context.KayitFormu.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            // Toggle active status
            registration.IsActive = !registration.IsActive;
            
            _context.Update(registration);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RegistrationExists(int id)
        {
            return await _context.KayitFormu.AnyAsync(e => e.Id == id);
        }
    }
} 
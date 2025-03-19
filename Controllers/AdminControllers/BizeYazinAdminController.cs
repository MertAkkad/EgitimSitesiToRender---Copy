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
    [Route("Admin/BizeYazin")]
    public class BizeYazinAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BizeYazinAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/KayitFormu
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var registrations = await _context.BizeYazin
                .OrderByDescending(k => k.CreationDate)
                .ToListAsync();
                
            return View("~/Views/Admin/BizeYazin/Index.cshtml", registrations);
        }

        // GET: Admin/KayitFormu/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.BizeYazin
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (registration == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/BizeYazin/Details.cshtml", registration);
        }

        // POST: Admin/KayitFormu/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registration = await _context.BizeYazin.FindAsync(id);
            
            if (registration != null)
            {
                _context.BizeYazin.Remove(registration);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/KayitFormu/Toggle/5
        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var registration = await _context.BizeYazin.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            // Toggle active status
            registration.Okundu = !registration.Okundu;
            
            _context.Update(registration);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RegistrationExists(int id)
        {
            return await _context.BizeYazin.AnyAsync(e => e.Id == id);
        }
    }
} 
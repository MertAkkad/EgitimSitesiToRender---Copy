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

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Subeler")]
    public class SubelerAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SubelerAdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Subeler
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var subeler = await _context.Subeler
                .OrderBy(s => s.Order)
                .ToListAsync();
                
            return View("~/Views/Admin/Subeler/Index.cshtml", subeler);
        }

        // POST: Admin/Subeler/Reorder
        [HttpPost("Reorder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderSubeler([FromBody] List<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
            {
                return BadRequest(new { success = false, message = "No IDs were provided." });
            }

            try
            {
                // Get all branch records that match the IDs
                var branchList = await _context.Subeler
                    .Where(s => itemIds.Contains(s.Id))
                    .ToListAsync();

                // Update order based on the position in the itemIds list
                foreach (var branch in branchList)
                {
                    int index = itemIds.IndexOf(branch.Id);
                    if (index != -1)
                    {
                        branch.Order = index + 1;
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

        // GET: Admin/Subeler/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sube = await _context.Subeler
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (sube == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Subeler/Details.cshtml", sube);
        }

        // GET: Admin/Subeler/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Subeler/Create.cshtml");
        }

        // POST: Admin/Subeler/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubeModel sube)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set order if not specified
                    if (sube.Order <= 0)
                    {
                        var maxOrder = await _context.Subeler.MaxAsync(s => (int?)s.Order) ?? 0;
                        sube.Order = maxOrder + 1;
                    }
                    else
                    {
                        // Shift other branches if this order is already taken
                        await ShiftBranchOrders(sube.Order);
                    }

                    // Set creation date
                    sube.CreationDate = DateTime.Now;

                    // Add to database
                    _context.Add(sube);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                }
            }

            return View("~/Views/Admin/Subeler/Create.cshtml", sube);
        }

        // GET: Admin/Subeler/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sube = await _context.Subeler.FindAsync(id);
            if (sube == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Subeler/Edit.cshtml", sube);
        }

        // POST: Admin/Subeler/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubeModel sube)
        {
            if (id != sube.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSube = await _context.Subeler.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    if (existingSube == null)
                    {
                        return NotFound();
                    }

                    // Check if order has changed
                    if (sube.Order != existingSube.Order)
                    {
                        await ShiftBranchOrders(sube.Order, sube.Id);
                    }

                    // Preserve creation date
                    sube.CreationDate = existingSube.CreationDate;

                    // Update the entity
                    _context.Update(sube);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BranchExists(sube.Id))
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

            return View("~/Views/Admin/Subeler/Edit.cshtml", sube);
        }

        // GET: Admin/Subeler/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sube = await _context.Subeler
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (sube == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Subeler/Delete.cshtml", sube);
        }

        // POST: Admin/Subeler/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sube = await _context.Subeler.FindAsync(id);
            
            _context.Subeler.Remove(sube);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<bool> BranchExists(int id)
        {
            return await _context.Subeler.AnyAsync(e => e.Id == id);
        }

        private async Task ShiftBranchOrders(int newOrder, int? excludeId = null)
        {
            var branchesToShift = await _context.Subeler
                .Where(s => s.Order >= newOrder && (excludeId == null || s.Id != excludeId))
                .OrderBy(s => s.Order)
                .ToListAsync();

            foreach (var branch in branchesToShift)
            {
                branch.Order++;
            }

            await _context.SaveChangesAsync();
        }

        // POST: Admin/Subeler/Toggle/5
        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var sube = await _context.Subeler.FindAsync(id);
            if (sube == null)
            {
                return NotFound();
            }

            // Toggle active status
            sube.IsActive = !sube.IsActive;
            
            _context.Update(sube);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Subeler/MoveUp/5
        [HttpPost("MoveUp/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var sube = await _context.Subeler.FindAsync(id);
            if (sube == null)
            {
                return NotFound();
            }
            
            // Find the item above this one in order
            var aboveBranch = await _context.Subeler
                .Where(s => s.Order < sube.Order)
                .OrderByDescending(s => s.Order)
                .FirstOrDefaultAsync();
                
            if (aboveBranch != null)
            {
                // Swap orders
                var tempOrder = sube.Order;
                sube.Order = aboveBranch.Order;
                aboveBranch.Order = tempOrder;
                
                _context.Update(sube);
                _context.Update(aboveBranch);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Subeler/MoveDown/5
        [HttpPost("MoveDown/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var sube = await _context.Subeler.FindAsync(id);
            if (sube == null)
            {
                return NotFound();
            }
            
            // Find the item below this one in order
            var belowBranch = await _context.Subeler
                .Where(s => s.Order > sube.Order)
                .OrderBy(s => s.Order)
                .FirstOrDefaultAsync();
                
            if (belowBranch != null)
            {
                // Swap orders
                var tempOrder = sube.Order;
                sube.Order = belowBranch.Order;
                belowBranch.Order = tempOrder;
                
                _context.Update(sube);
                _context.Update(belowBranch);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
} 
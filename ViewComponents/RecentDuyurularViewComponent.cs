using Microsoft.AspNetCore.Mvc;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace EgitimSitesi.ViewComponents
{
    public class RecentDuyurularViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RecentDuyurularViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? excludeId = null, int count = 5)
        {
            var recentDuyurular = await _context.Duyurular
                .Where(d => d.IsActive && (excludeId == null || d.Id != excludeId))
                .OrderByDescending(d => d.CreationDate)
                .Take(count)
                .ToListAsync();

            return View(recentDuyurular);
        }
    }
} 
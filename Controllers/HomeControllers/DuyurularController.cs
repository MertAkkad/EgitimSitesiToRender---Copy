using Microsoft.AspNetCore.Mvc;
using EgitimSitesi.Models;
using EgitimSitesi.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class DuyurularController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DuyurularController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Duyurular
        public async Task<IActionResult> Index()
        {
            // Get all active announcements ordered by CreationDate descending
            var duyurular = await _context.Duyurular
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.CreationDate)
                .ToListAsync();

            return View("~/Views/Home/HiddenPages/Duyurular.cshtml", duyurular);
        }

        // GET: /Duyurular/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var duyuru = await _context.Duyurular
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (duyuru == null)
            {
                return NotFound();
            }

            return View("~/Views/Home/HiddenPages/DuyuruDetay.cshtml", duyuru);
        }
    }
} 
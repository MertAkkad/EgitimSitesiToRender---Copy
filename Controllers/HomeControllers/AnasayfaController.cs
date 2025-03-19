using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EgitimSitesi.Models;
using EgitimSitesi.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class AnasayfaController : Controller
    {
        private readonly ILogger<AnasayfaController> _logger;
        private readonly ApplicationDbContext _context;

        public AnasayfaController(ILogger<AnasayfaController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get active banners ordered by Order field
            var banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.Order)
                .ToListAsync();

            // Get active content items ordered by Order field
            var contentItems = await _context.Set<Anasayfa_IcerikModel>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Order)
                .ToListAsync();
                
            // Get the latest 6 active announcements ordered by CreationDate descending
            var recentAnnouncements = await _context.Duyurular
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.CreationDate)
                .Take(6)
                .ToListAsync();
                
            // Get active courses ordered by Order field
            var courses = await _context.Kurslar
                .Where(k => k.IsActive)
                .OrderBy(k => k.Order)
                .ToListAsync();

            // Pass data to the view
            ViewBag.Banners = banners;
            ViewBag.ContentItems = contentItems;
            ViewBag.RecentAnnouncements = recentAnnouncements;
            ViewBag.Courses = courses;

            return View("~/Views/Home/Anasayfa/Index.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 
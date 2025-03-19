using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class KurslarimizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KurslarimizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Kurslarimiz
        public async Task<IActionResult> Index()
        {
            var allKurslar = await _context.Kurslar
                .Where(k => k.IsActive)
                .OrderBy(k => k.Order)
                .ToListAsync();

            var kurslarViewModel = new KurslarimizViewModel
            {
                AllKurslar = allKurslar,
                KursTurleri = allKurslar.Select(k => k.Type).Distinct().ToList()
            };

            return View("~/Views/Home/Kurslarimiz/Index.cshtml", kurslarViewModel);
        }

        // GET: /Home/Kurslarimiz/Detay/{id}
        public async Task<IActionResult> Detay(int id)
        {
            var kurs = await _context.Kurslar
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (kurs == null)
            {
                return NotFound();
            }

            return View("~/Views/Home/Kurslarimiz/Detay.cshtml", kurs);
        }

        // GET: /Home/Kurslarimiz/Tur/{type}
        public async Task<IActionResult> Tur(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return RedirectToAction(nameof(Index));
            }

            var kurslar = await _context.Kurslar
                .Where(k => k.IsActive && k.Type == type)
                .OrderBy(k => k.Order)
                .ToListAsync();

            var kurslarViewModel = new KurslarimizViewModel
            {
                AllKurslar = kurslar,
                KursTurleri = await _context.Kurslar
                    .Where(k => k.IsActive)
                    .Select(k => k.Type)
                    .Distinct()
                    .ToListAsync(),
                SelectedType = type
            };

            return View("~/Views/Home/Kurslarimiz/Index.cshtml", kurslarViewModel);
        }
    }

    public class KurslarimizViewModel
    {
        public List<KursModel> AllKurslar { get; set; } = new List<KursModel>();
        public List<string> KursTurleri { get; set; } = new List<string>();
        public string SelectedType { get; set; } = "";
    }
} 
using Microsoft.AspNetCore.Mvc;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class KayitFormuController : Controller
    {
        private readonly ILogger<KayitFormuController> _logger;
        private readonly ApplicationDbContext _context;

        public KayitFormuController(ILogger<KayitFormuController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: KayitFormu
        public IActionResult Index()
        {
            return View("~/Views/Home/KayıtFormu/Index.cshtml");
        }

        // POST: KayitFormu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(KayitFormuModel kayitFormu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values
                    kayitFormu.IsActive = true;
                    kayitFormu.CreationDate = DateTime.Now;

                    // Add to database
                    _context.KayitFormu.Add(kayitFormu);
                    await _context.SaveChangesAsync();

                    // Log success
                    _logger.LogInformation($"New registration submitted: {kayitFormu.Name}, {kayitFormu.Email}, {kayitFormu.Grade}");

                    // Show success message
                    ViewBag.Success = true;
                    return View("~/Views/Home/KayıtFormu/Index.cshtml");
                }
                catch (Exception ex)
                {
                    // Log error
                    _logger.LogError($"Error saving registration form: {ex.Message}");
                    ModelState.AddModelError("", "Kayıt formunuz gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
                }
            }

            return View("~/Views/Home/KayıtFormu/Index.cshtml", kayitFormu);
        }
    }
} 
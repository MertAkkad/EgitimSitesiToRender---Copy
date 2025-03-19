using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using System.Threading.Tasks;
using System.Linq;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class HakkimizdaController : Controller
    {
        private readonly ILogger<HakkimizdaController> _logger;
        private readonly ApplicationDbContext _context;

        public HakkimizdaController(ILogger<HakkimizdaController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hakkimizda = await _context.Hakkimizda
                .Where(h => h.IsActive)
                .FirstOrDefaultAsync();

            return View("~/Views/Home/Kurumsal/Hakkimizda/Index.cshtml", hakkimizda);
        }
    }
} 
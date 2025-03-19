using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class KadromuzController : Controller
    {
        private readonly ILogger<KadromuzController> _logger;
        private readonly ApplicationDbContext _context;

        public KadromuzController(ILogger<KadromuzController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var kadromuz = await _context.Kadromuz
                .Where(k => k.IsActive)
                .OrderBy(k => k.Title)
                .ThenBy(k => k.Grade)
                .ThenBy(k => k.Order)
                .ToListAsync();

            return View("~/Views/Home/Kurumsal/Kadromuz/Index.cshtml", kadromuz);
        }
    }
} 
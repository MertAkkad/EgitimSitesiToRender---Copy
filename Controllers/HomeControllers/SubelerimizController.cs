using EgitimSitesi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class SubelerimizController : Controller
    {
        private readonly ILogger<SubelerimizController> _logger;
        private readonly ApplicationDbContext _context;

        public SubelerimizController(ILogger<SubelerimizController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var subeler = await _context.Subeler
                .Where(s => s.IsActive)
                .OrderBy(s => s.Order)
                .ToListAsync();
                
            return View("~/Views/Home/Subelerimiz/Index.cshtml", subeler);
        }
    }
} 
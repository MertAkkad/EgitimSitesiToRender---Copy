using Microsoft.AspNetCore.Mvc;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class BasvuruFormuController : Controller
    {
        private readonly ILogger<BasvuruFormuController> _logger;

        public BasvuruFormuController(ILogger<BasvuruFormuController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Views/Home/BasvuruFormu/Index.cshtml");
        }

        [HttpPost]
        public IActionResult Submit(/*you can add form model parameters here*/)
        {
            // Process form submission
            // For now, just redirect back to form with success message
            TempData["SuccessMessage"] = "Başvurunuz başarıyla alınmıştır. En kısa sürede sizinle iletişime geçilecektir.";
            return RedirectToAction("Index");
        }
    }
} 
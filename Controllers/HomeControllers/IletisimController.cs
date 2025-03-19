using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using System.Linq;
using EgitimSitesi.Models;

namespace EgitimSitesi.Controllers.HomeControllers
{
    [Route("Iletisim")]
    public class IletisimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IletisimController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var iletisimBilgileri = await _context.Iletisim.ToListAsync();
            return View("~/Views/Home/Iletisim/Index.cshtml", iletisimBilgileri);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string name, string email, string phone, string subject, string message)
        {
            if (ModelState.IsValid)
            {
                var newMessage = new BizeYazinModel
                {
                    AdSoyad = name,
                    Email = email,
                    TelefonNo = phone,
                    Konu = subject,
                    Mesaj = message,
                    CreationDate = DateTime.Now,
                    IpAdresi = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
                };

                _context.BizeYazin.Add(newMessage);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mesajınız başarıyla gönderilmiştir";
            }
            else
            {
                TempData["ErrorMessage"] = "Mesaj gönderilirken bir hata oluştu. Lütfen tüm alanları doğru doldurduğunuzdan emin olun.";
            }
            return RedirectToAction("Index");
        }
    }
} 
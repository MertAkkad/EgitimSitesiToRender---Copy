using System.Linq;
using System.Threading.Tasks;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgitimSitesi.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public FooterViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var iletisimBilgileri = await _context.Iletisim
                .Where(i => i.IsActive)
                .FirstOrDefaultAsync();

            return View(iletisimBilgileri);
        }
    }
} 
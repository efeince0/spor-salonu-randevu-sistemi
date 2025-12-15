using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;

namespace SporSalonuRandevu.Controllers
{
    public class HizmetController : Controller
    {
        private readonly UygulamaDbContext _context;

        public HizmetController(UygulamaDbContext context)
        {
            _context = context;
        }

        // /Hizmet
        public IActionResult Index()
        {
            var hizmetler = _context.Hizmetler.ToList();
            return View(hizmetler);
        }
    }
}

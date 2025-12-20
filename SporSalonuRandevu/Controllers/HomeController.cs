using Microsoft.AspNetCore.Mvc;
using SporSalonuRandevu.Models;
using System.Diagnostics;

namespace SporSalonuRandevu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // spor salonu için randevu listleyen controller metodu
        public IActionResult RandevuListesi()
        {
            
            return View();
        }

    }
}

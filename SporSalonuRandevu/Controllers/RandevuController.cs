using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;
using System.Security.Claims;

namespace SporSalonuRandevu.Controllers
{
    [Authorize(Roles = "Uye")]
    public class RandevuController : Controller
    {
        private readonly UygulamaDbContext _context;

        public RandevuController(UygulamaDbContext context)
        {
            _context = context;
        }

        // 1️⃣ HİZMET SEÇ



        public IActionResult HizmetSec()
        {
            var hizmetler = _context.Hizmetler.ToList();
            return View(hizmetler);
        }

        [Authorize(Roles = "Uye")]
        public IActionResult AntrenorSec(int hizmetId)
        {
            // Veritabanındaki TÜM antrenörleri getiriyoruz.
            // Randevusu olup olmaması umurumuzda değil, çünkü hepsi listelenmeli.
            var antrenorler = _context.Antrenorler.ToList();

            ViewBag.HizmetId = hizmetId;

            // Listeyi sayfaya (View) gönderiyoruz
            return View(antrenorler);
        }

        [HttpGet("MusaitSaatler")]
        public IActionResult MusaitSaatler(int antrenorId, int hizmetId, DateTime tarih)
        {
            var hizmet = _context.Hizmetler.Find(hizmetId);
            if (hizmet == null) return BadRequest();

            int sure = hizmet.SureDakika;

            var doluSaatler = _context.Randevular
                .Where(r =>
                    r.AntrenorId == antrenorId &&
                    r.Tarih.Date == tarih.Date)
                .Select(r => r.Saat)
                .ToList();

            var musaitSaatler = new List<string>();

            for (int saat = 9; saat <= 20; saat++)
            {
                string saatStr = $"{saat:00}:00";

                if (!doluSaatler.Contains(saatStr))
                {
                    musaitSaatler.Add(saatStr);
                }
            }

            return Ok(musaitSaatler);
        }

        public IActionResult SaatSec(int antrenorId, int hizmetId, DateTime? tarih)
        {
            var antrenor = _context.Antrenorler
                .FirstOrDefault(a => a.Id == antrenorId);

            if (antrenor == null)
                return NotFound();

            var baslangic = antrenor.CalismaBaslangic; // TimeSpan
            var bitis = antrenor.CalismaBitis;         // TimeSpan

            var saatler = new List<TimeSpan>();

            for (var saat = baslangic; saat < bitis; saat = saat.Add(TimeSpan.FromHours(1)))
            {
                saatler.Add(saat);
            }

            ViewBag.Saatler = saatler;
            ViewBag.Tarih = tarih ?? DateTime.Today;
            ViewBag.AntrenorId = antrenorId;
            ViewBag.HizmetId = hizmetId;

            return View();
        }

      



    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;
using static SporSalonuRandevu.Models.Randevu;

namespace SporSalonuRandevu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UygulamaDbContext _context;

        public AdminController(UygulamaDbContext context)
        {
            _context = context;
        }

        // ADMIN DASHBOARD
        public IActionResult Index()
        {
            // Basit istatistikler
            ViewBag.HizmetSayisi = _context.Hizmetler.Count();
            ViewBag.AntrenorSayisi = _context.Antrenorler.Count();
            ViewBag.RandevuSayisi = _context.Randevular.Count();

            return View();
        }
        // -------------------------
        // HİZMET LİSTELE
        // -------------------------
        public IActionResult Hizmetler()
        {
            var hizmetler = _context.Hizmetler.ToList();
            return View(hizmetler);
        }

        // -------------------------
        // HİZMET EKLE (GET)
        // -------------------------
        public IActionResult HizmetEkle()
        {
            return View();
        }

        // -------------------------
        // HİZMET EKLE (POST)
        // -------------------------
        [HttpPost]
        public IActionResult HizmetEkle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
                return View(hizmet);

            _context.Hizmetler.Add(hizmet);
            _context.SaveChanges();

            return RedirectToAction(nameof(Hizmetler));
        }

        // -------------------------
        // HİZMET SİL
        // -------------------------
        public IActionResult HizmetSil(int id)
        {
            var hizmet = _context.Hizmetler.Find(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Hizmetler));
        }
        // -------------------------
        // HİZMET GÜNCELLE (GET)
        // -------------------------
        public IActionResult HizmetGuncelle(int id)
        {
            var hizmet = _context.Hizmetler.Find(id);
            if (hizmet == null)
                return NotFound();

            return View(hizmet);
        }

        // -------------------------
        // HİZMET GÜNCELLE (POST)
        // -------------------------
        [HttpPost]
        public IActionResult HizmetGuncelle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
                return View(hizmet);

            _context.Hizmetler.Update(hizmet);
            _context.SaveChanges();

            return RedirectToAction(nameof(Hizmetler));
        }

        // =========================
        // ANTRONÖR LİSTELE
        // =========================
        public IActionResult Antrenorler()
        {
            return View(_context.Antrenorler.ToList());
        }

        // =========================
        // EKLE (GET)
        // =========================
        public IActionResult AntrenorEkle()
        {
            return View();
        }

        // =========================
        // EKLE (POST)
        // =========================

        [HttpPost]
        public IActionResult AntrenorEkle(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
                return View(antrenor);

            _context.Antrenorler.Add(antrenor);
            _context.SaveChanges();

            return RedirectToAction("Antrenorler");
        }


        // =========================
        // GÜNCELLE (GET)
        // =========================
        public IActionResult AntrenorGuncelle(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            return View(antrenor);
        }

        // =========================
        // GÜNCELLE (POST)
        // =========================
        [HttpPost]
        public IActionResult AntrenorGuncelle(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
                return View(antrenor);

            _context.Antrenorler.Update(antrenor);
            _context.SaveChanges();

            return RedirectToAction("Antrenorler");
        }


        // =========================
        // SİL
        // =========================
        public IActionResult AntrenorSil(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            _context.Antrenorler.Remove(antrenor);
            _context.SaveChanges();
            return RedirectToAction("Antrenorler");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RandevuYonetimi()
        {
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .OrderByDescending(r => r.Tarih)
                .ToList();

            return View(randevular);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RandevuOnayla(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Hizmet)
                .FirstOrDefault(r => r.Id == id);

            if (randevu == null)
                return NotFound();

            randevu.Durum = RandevuDurumu.Onaylandi;

            // 🔔 BİLDİRİM OLUŞTUR
            _context.Bildirimler.Add(new Bildirim
            {
                UyeId = randevu.UyeId,
                Mesaj = $"✅ {randevu.Tarih:dd.MM.yyyy} tarihli {randevu.Saat} saatli " +
                        $"{randevu.Hizmet.Ad} randevunuz ONAYLANDI."
            });

            _context.SaveChanges();

            return RedirectToAction("RandevuYonetimi");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RandevuIptal(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Hizmet)
                .FirstOrDefault(r => r.Id == id);

            if (randevu == null)
                return NotFound();

            randevu.Durum = RandevuDurumu.IptalEdildi;

            // 🔔 BİLDİRİM OLUŞTUR
            _context.Bildirimler.Add(new Bildirim
            {
                UyeId = randevu.UyeId,
                Mesaj = $"❌ {randevu.Tarih:dd.MM.yyyy} tarihli {randevu.Saat} saatli " +
                        $"{randevu.Hizmet.Ad} randevunuz İPTAL EDİLDİ."
            });

            _context.SaveChanges();

            return RedirectToAction("RandevuYonetimi");
        }




    }
}
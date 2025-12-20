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

        // DASHBOARD
        public IActionResult Index()
        {
            ViewBag.HizmetSayisi = _context.Hizmetler.Count();
            ViewBag.AntrenorSayisi = _context.Antrenorler.Count();

            ViewBag.RandevuSayisi = _context.Randevular.Count(r =>
                r.Durum == RandevuDurumu.Beklemede ||
                r.Durum == RandevuDurumu.Onaylandi
            );

            ViewBag.OnaylananRandevu = _context.Randevular.Count(r =>
                r.Durum == RandevuDurumu.Onaylandi
            );

            ViewBag.BekleyenRandevu = _context.Randevular.Count(r =>
                r.Durum == RandevuDurumu.Beklemede
            );

            var bugun = DateTime.Today;
            var birHaftaOnce = bugun.AddDays(-7);

            ViewBag.HaftalikCiro = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r =>
                    r.Durum == RandevuDurumu.Onaylandi &&
                    r.Tarih >= birHaftaOnce &&
                    r.Tarih <= bugun
                )
                .Sum(r => (decimal?)r.Hizmet.Ucret) ?? 0;

            ViewBag.PotansiyelCiro = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r =>
                    r.Durum == RandevuDurumu.Beklemede ||
                    (r.Durum == RandevuDurumu.Onaylandi && r.Tarih > bugun)
                )
                .Sum(r => (decimal?)r.Hizmet.Ucret) ?? 0;

            return View();
        }

        // HİZMETLER
        public IActionResult Hizmetler()
        {
            return View(_context.Hizmetler.ToList());
        }

        public IActionResult HizmetEkle()
        {
            return View();
        }

        [HttpPost]// ekle post
        public IActionResult HizmetEkle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
                return View(hizmet);

            _context.Hizmetler.Add(hizmet);
            _context.SaveChanges();
            return RedirectToAction(nameof(Hizmetler));
        }

        public IActionResult HizmetGuncelle(int id)
        {
            var hizmet = _context.Hizmetler.Find(id);
            if (hizmet == null) return NotFound();
            return View(hizmet);
        }
        // hizmet güncelle postu
        [HttpPost]
        public IActionResult HizmetGuncelle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
                return View(hizmet);

            _context.Hizmetler.Update(hizmet);
            _context.SaveChanges();
            return RedirectToAction(nameof(Hizmetler));
        }
        
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

        // ANTRENÖRLER
        public IActionResult Antrenorler()
        {
            return View(_context.Antrenorler.ToList());
        }

        public IActionResult AntrenorEkle()
        {
            return View();
        }
        //güncelleme get
        [HttpPost]
        public IActionResult AntrenorEkle(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
                return View(antrenor);

            _context.Antrenorler.Add(antrenor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Antrenorler));
        }

        public IActionResult AntrenorGuncelle(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }
        //post gğncelle
        [HttpPost]
        public IActionResult AntrenorGuncelle(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
                return View(antrenor);

            _context.Antrenorler.Update(antrenor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Antrenorler));
        }

        public IActionResult AntrenorSil(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Antrenorler));
        }

        // RANDEVU YÖNETİMİ
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
        //ONAY
        public IActionResult RandevuOnayla(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Hizmet)
                .FirstOrDefault(r => r.Id == id);

            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Onaylandi;

            _context.Bildirimler.Add(new Bildirim
            {
                UyeId = randevu.UyeId,
                Mesaj = $"✅ {randevu.Tarih:dd.MM.yyyy} {randevu.Saat} - {randevu.Hizmet.Ad} randevunuz ONAYLANDI."// bildirim için
            });

            _context.SaveChanges();
            return RedirectToAction(nameof(RandevuYonetimi));
        }
        //İPTAL
        public IActionResult RandevuIptal(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Hizmet)
                .FirstOrDefault(r => r.Id == id);

            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.IptalEdildi;

            _context.Bildirimler.Add(new Bildirim
            {
                UyeId = randevu.UyeId,
                Mesaj = $"❌ {randevu.Tarih:dd.MM.yyyy} {randevu.Saat} - {randevu.Hizmet.Ad} randevunuz İPTAL EDİLDİ."//bildirim için
            });

            _context.SaveChanges();
            return RedirectToAction(nameof(RandevuYonetimi));
        }
    }
}

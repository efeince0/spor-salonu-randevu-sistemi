using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;
using static SporSalonuRandevu.Models.Randevu;

namespace SporSalonuRandevu.Controllers
{
    // Sadece Admin rolündeki kullanıcılar erişebilir
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UygulamaDbContext _context;

        // DbContext injection
        public AdminController(UygulamaDbContext context)
        {
            _context = context;
        }

        // ADMIN DASHBOARD
        public IActionResult Index()
        {
            // Toplam hizmet sayısı
            ViewBag.HizmetSayisi = _context.Hizmetler.Count();

            // Toplam antrenör sayısı
            ViewBag.AntrenorSayisi = _context.Antrenorler.Count();

            // Aktif (bekleyen + onaylanan) randevu sayısı
            ViewBag.RandevuSayisi = _context.Randevular.Count(r =>
                r.Durum == RandevuDurumu.Beklemede ||
                r.Durum == RandevuDurumu.Onaylandi
            );

            // Onaylanan randevular
            ViewBag.OnaylananRandevu = _context.Randevular.Count(r =>
                r.Durum == RandevuDurumu.Onaylandi
            );

            // Bekleyen randevular
            ViewBag.BekleyenRandevu = _context.Randevular.Count(r =>
                r.Durum == RandevuDurumu.Beklemede
            );

            // Tarih aralığı (son 7 gün)
            var bugun = DateTime.Today;
            var birHaftaOnce = bugun.AddDays(-7);

            // Son 7 gündeki onaylanmış randevuların toplam cirosu
            ViewBag.HaftalikCiro = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r =>
                    r.Durum == RandevuDurumu.Onaylandi &&
                    r.Tarih >= birHaftaOnce &&
                    r.Tarih <= bugun
                )
                .Sum(r => (decimal?)r.Hizmet.Ucret) ?? 0;

            // Bekleyen ve ileri tarihli onaylı randevuların potansiyel cirosu
            ViewBag.PotansiyelCiro = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r =>
                    r.Durum == RandevuDurumu.Beklemede ||
                    (r.Durum == RandevuDurumu.Onaylandi && r.Tarih > bugun)
                )
                .Sum(r => (decimal?)r.Hizmet.Ucret) ?? 0;

            return View();
        }

        // HİZMET LİSTESİ
        public IActionResult Hizmetler()
        {
            return View(_context.Hizmetler.ToList());
        }

        // HİZMET EKLE (GET)
        public IActionResult HizmetEkle()
        {
            return View();
        }

        // HİZMET EKLE (POST)
        [HttpPost]
        public IActionResult HizmetEkle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
                return View(hizmet);

            _context.Hizmetler.Add(hizmet);
            _context.SaveChanges();
            return RedirectToAction(nameof(Hizmetler));
        }

        // HİZMET GÜNCELLE (GET)
        public IActionResult HizmetGuncelle(int id)
        {
            var hizmet = _context.Hizmetler.Find(id);
            if (hizmet == null) return NotFound();
            return View(hizmet);
        }

        // HİZMET GÜNCELLE (POST)
        [HttpPost]
        public IActionResult HizmetGuncelle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
                return View(hizmet);

            _context.Hizmetler.Update(hizmet);
            _context.SaveChanges();
            return RedirectToAction(nameof(Hizmetler));
        }

        // HİZMET SİL
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

        // ANTRENÖR LİSTESİ
        public IActionResult Antrenorler()
        {
            return View(_context.Antrenorler.ToList());
        }

        // ANTRENÖR EKLE (GET)
        public IActionResult AntrenorEkle()
        {
            return View();
        }

        // ANTRENÖR EKLE (POST)
        [HttpPost]
        public IActionResult AntrenorEkle(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
                return View(antrenor);

            _context.Antrenorler.Add(antrenor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Antrenorler));
        }

        // ANTRENÖR GÜNCELLE (GET)
        public IActionResult AntrenorGuncelle(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        // ANTRENÖR GÜNCELLE (POST)
        [HttpPost]
        public IActionResult AntrenorGuncelle(Antrenor antrenor)
        {
            if (!ModelState.IsValid)
                return View(antrenor);

            _context.Antrenorler.Update(antrenor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Antrenorler));
        }

        // ANTRENÖR SİL
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

        // RANDEVU ONAYLA
        public IActionResult RandevuOnayla(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Hizmet)
                .FirstOrDefault(r => r.Id == id);

            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Onaylandi;

            // Üyeye bildirim gönder
            _context.Bildirimler.Add(new Bildirim
            {
                UyeId = randevu.UyeId,
                Mesaj = $"✅ {randevu.Tarih:dd.MM.yyyy} {randevu.Saat} - {randevu.Hizmet.Ad} randevunuz ONAYLANDI."
            });

            _context.SaveChanges();
            return RedirectToAction(nameof(RandevuYonetimi));
        }

        // RANDEVU İPTAL
        public IActionResult RandevuIptal(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Hizmet)
                .FirstOrDefault(r => r.Id == id);

            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.IptalEdildi;

            // Üyeye iptal bildirimi
            _context.Bildirimler.Add(new Bildirim
            {
                UyeId = randevu.UyeId,
                Mesaj = $"❌ {randevu.Tarih:dd.MM.yyyy} {randevu.Saat} - {randevu.Hizmet.Ad} randevunuz İPTAL EDİLDİ."
            });

            _context.SaveChanges();
            return RedirectToAction(nameof(RandevuYonetimi));
        }
    }
}

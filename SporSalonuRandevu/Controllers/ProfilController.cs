using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;
using System.Security.Claims;

[Authorize(Roles = "Uye")]
public class ProfilController : Controller
{
    private readonly UygulamaDbContext _context;
    private readonly UserManager<Uye> _userManager;

    public ProfilController(UygulamaDbContext context, UserManager<Uye> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // PROFİL SAYFASI (Ana)
    public IActionResult Index()
    {
        return View();
    }

  
    // PROFİL BİLGİLERİ
    [Authorize]
    public async Task<IActionResult> Bilgilerim()
    {
        var uye = await _userManager.GetUserAsync(User);
        if (uye == null)
            return RedirectToAction("Login", "Account");

        return View(uye);
    }

    [HttpPost]
    public async Task<IActionResult> Bilgilerim(
     string adSoyad,
     int? yas,
     int? boy,
     int? kilo)
    {
        var uye = await _userManager.GetUserAsync(User);
        if (uye == null) return RedirectToAction("Login", "Account");

        uye.AdSoyad = adSoyad;
        uye.Yas = yas;
        uye.Boy = boy;
        uye.Kilo = kilo;

        await _userManager.UpdateAsync(uye);

        ViewBag.Mesaj = "Profil bilgileriniz güncellendi.";
        return View(uye);
    }

    // ŞİFRE DEĞİŞTİR
    public IActionResult SifreDegistir()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SifreDegistir(string eskiSifre, string yeniSifre)
    {
        var uye = await _userManager.GetUserAsync(User);

        var sonuc = await _userManager.ChangePasswordAsync(
            uye, eskiSifre, yeniSifre);

        if (!sonuc.Succeeded)
        {
            ViewBag.Hata = "Şifre değiştirilemedi";
            return View();
        }

        ViewBag.Mesaj = "Şifre başarıyla değiştirildi";
        return View();
    }

// RANDEVULARIM
    public async Task<IActionResult> Randevularim()
    {
        var uyeId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var randevular = _context.Randevular
            .Include(r => r.Antrenor)
            .Include(r => r.Hizmet)
            .Where(r => r.UyeId == uyeId)
            .OrderByDescending(r => r.Tarih)
            .ToList();

        return View(randevular);
    }

   
    // ÜYE RANDEVU İPTAL
  
    public IActionResult RandevuIptal(int id)
    {
        var uyeId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var randevu = _context.Randevular
            .FirstOrDefault(r => r.Id == id && r.UyeId == uyeId);

        if (randevu == null) return NotFound();

        randevu.Durum = RandevuDurumu.IptalEdildi;
        _context.SaveChanges();

        return RedirectToAction("Randevularim");
    }


    [Authorize(Roles = "Uye")]
    public IActionResult RandevuGuncelle(int id)
    {
        var uyeId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

        var randevu = _context.Randevular
            .Include(r => r.Antrenor)
            .Include(r => r.Hizmet)
            .FirstOrDefault(r => r.Id == id && r.UyeId == uyeId);

        if (randevu == null)
            return NotFound();

        if (randevu.Durum != RandevuDurumu.Beklemede)
            return BadRequest("Bu randevu güncellenemez.");

        return View(randevu);
    }


    [HttpPost]
    [Authorize(Roles = "Uye")]
    public IActionResult RandevuGuncelle(int id, DateTime tarih, string saat)
    {
        var uyeId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

        var randevu = _context.Randevular
            .FirstOrDefault(r => r.Id == id && r.UyeId == uyeId);

        if (randevu == null)
            return NotFound();

        //  İptal edilmiş randevu güncellenemez
        if (randevu.Durum == RandevuDurumu.IptalEdildi)
            return BadRequest("İptal edilmiş randevu güncellenemez.");

        //  TARİH + SAAT GÜNCELLE
        randevu.Tarih = tarih;
        randevu.Saat = saat;

        //  GÜNCELLEME VARSA TEKRAR ONAY BEKLESİN
        randevu.Durum = RandevuDurumu.Beklemede;

        _context.SaveChanges();

        return RedirectToAction("Randevularim");
    }

    [Authorize(Roles = "Uye")]
    public IActionResult Bildirimler()
    {
        var uyeId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var bildirimler = _context.Bildirimler
            .Where(b => b.UyeId == uyeId)
            .OrderByDescending(b => b.OlusturmaTarihi)
            .ToList();

        return View(bildirimler);
    }

    [Authorize(Roles = "Uye")]
    public IActionResult BildirimOkundu(int id)
    {
        var bildirim = _context.Bildirimler.Find(id);
        if (bildirim == null) return NotFound();

        bildirim.OkunduMu = true;
        _context.SaveChanges();

        return RedirectToAction("Bildirimler");
    }



}

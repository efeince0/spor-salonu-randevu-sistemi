using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalonuRandevu.Models;
using SporSalonuRandevu.Services;

namespace SporSalonuRandevu.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly UserManager<Uye> _userManager;
        private readonly GeminiService _geminiService;

        public AiController(
            UserManager<Uye> userManager,
            GeminiService geminiService)
        {
            _userManager = userManager;
            _geminiService = geminiService;
        }

        // =======================
        // GET (Sayfa Açılışı)
        // =======================
        [HttpGet]
        public async Task<IActionResult> DiyetEgzersiz()
        {
            var uye = await _userManager.GetUserAsync(User);

            // 1. Session'da daha önce kaydedilmiş bir plan var mı kontrol et
            var kayitliPlan = HttpContext.Session.GetString("DiyetPlani");

            if (!string.IsNullOrEmpty(kayitliPlan))
            {
                // Varsa ViewBag'e at, böylece sayfa yenilense de plan ekranda kalır
                ViewBag.Plan = kayitliPlan;
            }

            return View(uye);
        }

        // =======================
        // POST (Form Gönderimi)
        // =======================
        [HttpPost]
        public async Task<IActionResult> DiyetEgzersiz(double hedefKilo, string ekNotlar)
        {
            var uye = await _userManager.GetUserAsync(User);

            if (uye == null)
                return RedirectToAction("Login", "Account");

            // ---- PROFİL BİLGİ KONTROLÜ ----
            if (!uye.Boy.HasValue || !uye.Kilo.HasValue || !uye.Yas.HasValue)
            {
                ViewBag.Hata = "❗ Profil bilgileriniz eksik. Lütfen profil sayfasından boy, kilo ve yaş bilgilerinizi doldurun.";
                return View(uye);
            }

            if (hedefKilo <= 0)
            {
                ViewBag.Hata = "❗ Lütfen geçerli bir hedef kilo giriniz.";
                return View(uye);
            }

            // ---- HESAPLAMALAR ----
            double boyMetre = uye.Boy.Value / 100.0;
            double kilo = uye.Kilo.Value;
            double bmi = kilo / (boyMetre * boyMetre);
            double idealMin = 18.5 * boyMetre * boyMetre;
            double idealMax = 24.9 * boyMetre * boyMetre;

            // ---- AI ÇAĞRISI ----
            string aiCevap;

            try
            {
                // Servise "ekNotlar"ı da gönderiyoruz
                aiCevap = await _geminiService.DiyetVeEgzersizOlustur(
                    uye.Yas.Value,
                    (int)uye.Boy.Value,   // 'double' olan değeri 'int'e çevirdik
                    (int)uye.Kilo.Value,  // 'double' olan değeri 'int'e çevirdik
                    hedefKilo,
                    bmi,
                    idealMin,
                    idealMax,
                    ekNotlar
                );

            }
            catch (Exception ex)
            {
                aiCevap = $"⚠️ HATA OLUŞTU: {ex.Message}";
                if (ex.InnerException != null)
                {
                    aiCevap += $"\n DETAY: {ex.InnerException.Message}";
                }
            }

            // 2. Sonucu Session'a kaydet (Kalıcı olması için)
            HttpContext.Session.SetString("DiyetPlani", aiCevap);

            // 3. PRG (Post-Redirect-Get) Deseni:
            // Doğrudan View döndürmek yerine Redirect yapıyoruz.
            // Bu sayede sayfa yenilendiğinde "Formu tekrar gönder?" uyarısı çıkmaz ve veri kaybolmaz.
            return RedirectToAction("DiyetEgzersiz");
        }

        // =======================
        // PLAN TEMİZLEME
        // =======================
        [HttpGet]
        public IActionResult PlaniTemizle()
        {
            // Session'daki veriyi sil
            HttpContext.Session.Remove("DiyetPlani");

            // Sayfayı yenile
            return RedirectToAction("DiyetEgzersiz");
        }
    }
}
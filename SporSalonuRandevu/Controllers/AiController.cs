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

        // get
        [HttpGet]
        public async Task<IActionResult> DiyetEgzersiz()
        {
            var uye = await _userManager.GetUserAsync(User);

            // 1. Session'da daha önce kaydedilmiş bir plan var mı kontrol edilir
            var kayitliPlan = HttpContext.Session.GetString("DiyetPlani");

            if (!string.IsNullOrEmpty(kayitliPlan))
            {
                // Varsa ViewBag'e at, böylece sayfa yenilense de plan ekranda kalsın
                ViewBag.Plan = kayitliPlan;
            }

            return View(uye);
        }


        
        [HttpPost] //post
        public async Task<IActionResult> DiyetEgzersiz(double hedefKilo, string ekNotlar)
        {
            var uye = await _userManager.GetUserAsync(User);

            if (uye == null)
                return RedirectToAction("Login", "Account");

            // profil bilgileri eksik mi kontrolü
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

            //  HESAPLAMALAR 
            double boyMetre = uye.Boy.Value / 100.0;
            double kilo = uye.Kilo.Value;
            double bmi = kilo / (boyMetre * boyMetre);
            double idealMin = 18.5 * boyMetre * boyMetre;
            double idealMax = 24.9 * boyMetre * boyMetre;

            // ai çağrısı
            string aiCevap;

            try
            {
               
                aiCevap = await _geminiService.DiyetVeEgzersizOlustur(
                    uye.Yas.Value,
                    (int)uye.Boy.Value,   
                    (int)uye.Kilo.Value,  
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

            //  Session'a kaydet (Kalıcı olması için)
            HttpContext.Session.SetString("DiyetPlani", aiCevap);

           
            return RedirectToAction("DiyetEgzersiz");
        }

   
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
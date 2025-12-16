using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;

namespace SporSalonuRandevu.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandevuApiController : ControllerBase
    {
        private readonly UygulamaDbContext _context;

        public RandevuApiController(UygulamaDbContext context)
        {
            _context = context;
        }

        // 🔥 LINQ KULLANAN API
        // Seçilen hizmete göre randevusu olmayan antrenörleri getirir
        [HttpGet("MusaitAntrenorler")]
        public IActionResult MusaitAntrenorler(int hizmetId)
        {
            var antrenorler = _context.Antrenorler
                .Where(a => !_context.Randevular
                    .Any(r => r.AntrenorId == a.Id && r.HizmetId == hizmetId))
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.Uzmanlik
                })
                .ToList();

            return Ok(antrenorler);
        }




        [HttpGet("MusaitSaatler")]
        public IActionResult MusaitSaatler(int antrenorId, int hizmetId, DateTime tarih)
        {
            var antrenor = _context.Antrenorler.FirstOrDefault(a => a.Id == antrenorId);
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == hizmetId);

            if (antrenor == null || hizmet == null) return BadRequest("Veri bulunamadı.");

            var calismaBaslangic = antrenor.CalismaBaslangic;
            var calismaBitis = antrenor.CalismaBitis;
            var talepEdilenSure = TimeSpan.FromMinutes(hizmet.SureDakika);

            // --- KRİTİK DÜZELTME BURASI ---
            // Eğer bitiş saati, başlangıçtan küçük veya eşitse (Örn: Başlangıç 12:00, Bitiş 00:00)
            // Bitiş saatini 24 saat ileri (bir sonraki gün) olarak ayarla.
            // Böylece 00:00 saati matematiksel olarak 24:00 olur ve döngü çalışır.
            if (calismaBitis <= calismaBaslangic)
            {
                calismaBitis = calismaBitis.Add(TimeSpan.FromDays(1));
            }
            // -----------------------------

            var randevularRaw = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == antrenorId && r.Tarih.Date == tarih.Date)
                .Select(r => new { BaslangicString = r.Saat, Sure = r.Hizmet.SureDakika })
                .ToList();

            var doluAraliklar = randevularRaw.Select(r => new
            {
                Baslang = TimeSpan.Parse(r.BaslangicString),
                Bitis = TimeSpan.Parse(r.BaslangicString).Add(TimeSpan.FromMinutes(r.Sure))
            }).ToList();

            var uygunSaatler = new List<string>();

            for (var suankiSaat = calismaBaslangic;
                 suankiSaat + talepEdilenSure <= calismaBitis;
                 suankiSaat = suankiSaat.Add(TimeSpan.FromHours(1)))
            {
                var adayBaslangic = suankiSaat;
                var adayBitis = suankiSaat + talepEdilenSure;

                bool cakismaVar = doluAraliklar.Any(dolu =>
                    adayBaslangic < dolu.Bitis && adayBitis > dolu.Baslang
                );

                if (!cakismaVar)
                {
                    // Saat 24:00 ve üzeriyse (gece yarısını geçtiyse) mod alarak saati düzelt (25:00 -> 01:00 gibi)
                    // Ama senin durumunda 23:00 en son slot olacağı için normal çalışır.
                    uygunSaatler.Add(suankiSaat.ToString(@"hh\:mm"));
                }
            }

            return Ok(uygunSaatler);
        }



        // 1. BU SINIFI namespace'in içine ama class'ın dışına (veya en alta) ekle
        public class RandevuEkleModel
        {
            public int AntrenorId { get; set; }
            public int HizmetId { get; set; }
            public DateTime Tarih { get; set; }
            public string Saat { get; set; }
        }

        // 2. BU METODU Controller class'ının içine, MusaitSaatler'in altına ekle
        [HttpPost("randevu-olustur")]
        public IActionResult RandevuOlustur([FromBody] RandevuEkleModel model)
        {
            // A) Giriş yapan kullanıcıyı bul (Identity kullanıyorsan)
            // Eğer User.FindFirst null geliyorsa, giriş yapılmamış demektir.
            var uyeId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uyeId))
            {
                return Unauthorized(new { mesaj = "Lütfen önce giriş yapınız." });
            }

            // B) Hizmet süresini veya detaylarını kontrol edebilirsin (Opsiyonel)
            var hizmet = _context.Hizmetler.Find(model.HizmetId);
            if (hizmet == null) return BadRequest("Hizmet bulunamadı.");

            // C) Yeni Randevu Nesnesini Oluştur
            var yeniRandevu = new Randevu
            {
                AntrenorId = model.AntrenorId,
                HizmetId = model.HizmetId,
                UyeId = uyeId,        // Giriş yapan üye
                Tarih = model.Tarih,
                Saat = model.Saat,    // Seçilen saat (Örn: "14:00")
                                      // Eğer tablonda 'OlusturulmaTarihi' gibi bir alan varsa:
                                      // CreatedDate = DateTime.Now 
            };

            // D) Veritabanına Ekle ve Kaydet
            _context.Randevular.Add(yeniRandevu);
            _context.SaveChanges();

            return Ok(new { mesaj = "Randevu başarıyla oluşturuldu!" });
        }

    }
}

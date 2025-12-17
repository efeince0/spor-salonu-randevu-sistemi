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
            // 1️⃣ GEÇMİŞ TARİH KONTROLÜ (YENİ)
            // Eğer seçilen tarih bugünden önceyse direkt boş liste dön.
            if (tarih.Date < DateTime.Today)
            {
                return Ok(new List<string>());
            }

            var antrenor = _context.Antrenorler.FirstOrDefault(a => a.Id == antrenorId);
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == hizmetId);

            if (antrenor == null || hizmet == null) return BadRequest("Veri bulunamadı.");

            var calismaBaslangic = antrenor.CalismaBaslangic;
            var calismaBitis = antrenor.CalismaBitis;
            var talepEdilenSure = TimeSpan.FromMinutes(hizmet.SureDakika);

            // Gece yarısı geçiş düzeltmesi
            if (calismaBitis <= calismaBaslangic)
            {
                calismaBitis = calismaBitis.Add(TimeSpan.FromDays(1));
            }

            var randevularRaw = _context.Randevular
               .Include(r => r.Hizmet)
               .Where(r =>
                   r.AntrenorId == antrenorId &&
                   r.Tarih.Date == tarih.Date &&
                   r.Durum != RandevuDurumu.IptalEdildi   // 🔥 KRİTİK SATIR
               )
               .Select(r => new
               {
                   BaslangicString = r.Saat,
                   Sure = r.Hizmet.SureDakika
               })
               .ToList();


            var doluAraliklar = randevularRaw.Select(r => new
            {
                Baslang = TimeSpan.Parse(r.BaslangicString),
                Bitis = TimeSpan.Parse(r.BaslangicString).Add(TimeSpan.FromMinutes(r.Sure))
            }).ToList();

            var uygunSaatler = new List<string>();

            // Şu anki zamanı alıyoruz (Sadece saat kısmı)
            var suankiAn = DateTime.Now.TimeOfDay;
            for (var suankiSaat = calismaBaslangic;
                 suankiSaat + talepEdilenSure <= calismaBitis;
                 suankiSaat = suankiSaat.Add(TimeSpan.FromHours(1)))
            {
                // 🔴 BUGÜN GEÇMİŞ SAATLERİ KESİN ENGELLE
                if (tarih.Date == DateTime.Today)
                {
                    var simdi = DateTime.Now.TimeOfDay;
                    if (suankiSaat.Hours <= simdi.Hours)
                        continue;
                }

                var adayBaslangic = suankiSaat;
                var adayBitis = suankiSaat + talepEdilenSure;

                bool cakismaVar = doluAraliklar.Any(dolu =>
                    adayBaslangic < dolu.Bitis &&
                    +adayBitis > dolu.Baslang
                );

                if (!cakismaVar)
                {
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
            var uyeId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uyeId))
            {
                return Unauthorized(new { mesaj = "Lütfen önce giriş yapınız." });
            }

            // 1️⃣ GEÇMİŞ TARİH/SAAT ENGELİ (YENİ)
            // Seçilen tarih bugünden eskiyse HATA VER.
            if (model.Tarih.Date < DateTime.Today)
            {
                return BadRequest("Geçmiş tarihe randevu alınamaz.");
            }

            // Eğer tarih BUGÜN ise ve saat şu andan eskiyse HATA VER.
            if (model.Tarih.Date == DateTime.Today)
            {
                TimeSpan secilenSaat = TimeSpan.Parse(model.Saat);
                if (secilenSaat < DateTime.Now.TimeOfDay)
                {
                    return BadRequest("Geçmiş saate randevu alınamaz.");
                }
            }

            var hizmet = _context.Hizmetler.Find(model.HizmetId);
            if (hizmet == null) return BadRequest("Hizmet bulunamadı.");

            var yeniRandevu = new Randevu
            {
                AntrenorId = model.AntrenorId,
                HizmetId = model.HizmetId,
                UyeId = uyeId,
                Tarih = model.Tarih,
                Saat = model.Saat,
                Durum = RandevuDurumu.Beklemede
            };


            _context.Randevular.Add(yeniRandevu);
            _context.SaveChanges();

            return Ok(new { mesaj = "Randevu başarıyla oluşturuldu!" });
        }

    }
}

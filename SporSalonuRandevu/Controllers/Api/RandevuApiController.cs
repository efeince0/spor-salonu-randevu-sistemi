using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;
using System.Security.Claims;

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

        //  MÜSAİT ANTRENÖRLER
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

        //  MÜSAİT SAATLER
        [HttpGet("MusaitSaatler")]
        public IActionResult MusaitSaatler(
            int antrenorId,
            int hizmetId,
            DateTime tarih,
            int? randevuId = null 
        )
        {
            //  Geçmiş tarih engeli
            if (tarih.Date < DateTime.Today)
            {
                return Ok(new List<string>());
            }

            var antrenor = _context.Antrenorler.FirstOrDefault(a => a.Id == antrenorId);
            var hizmet = _context.Hizmetler.FirstOrDefault(h => h.Id == hizmetId);

            if (antrenor == null || hizmet == null)
                return BadRequest("Veri bulunamadı.");

            var calismaBaslangic = antrenor.CalismaBaslangic;
            var calismaBitis = antrenor.CalismaBitis;
            var talepEdilenSure = TimeSpan.FromMinutes(hizmet.SureDakika);

            
            if (calismaBitis <= calismaBaslangic)
            {
                calismaBitis = calismaBitis.Add(TimeSpan.FromDays(1));
            }

            //  DOLU RANDEVULAR
            var randevularRaw = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r =>
                    r.AntrenorId == antrenorId &&
                    r.Tarih.Date == tarih.Date &&
                    r.Durum != RandevuDurumu.IptalEdildi &&
                    (randevuId == null || r.Id != randevuId) //  KENDİ RANDEVUSUNU HARİÇ TUT
                )
                .Select(r => new
                {
                    Baslangic = TimeSpan.Parse(r.Saat),
                    Bitis = TimeSpan.Parse(r.Saat)
                        .Add(TimeSpan.FromMinutes(r.Hizmet.SureDakika))
                })
                .ToList();

            var uygunSaatler = new List<string>();

            for (var saat = calismaBaslangic;
                 saat + talepEdilenSure <= calismaBitis;
                 saat = saat.Add(TimeSpan.FromHours(1)))
            {
                //  Bugün geçmiş saat engeli
                if (tarih.Date == DateTime.Today)
                {
                    if (saat <= DateTime.Now.TimeOfDay)
                        continue;
                }

                var adayBaslangic = saat;
                var adayBitis = saat + talepEdilenSure;

                bool cakismaVar = randevularRaw.Any(dolu =>
                    adayBaslangic < dolu.Bitis &&
                    adayBitis > dolu.Baslangic
                );

                if (!cakismaVar)
                {
                    uygunSaatler.Add(saat.ToString(@"hh\:mm"));
                }
            }

            return Ok(uygunSaatler);
        }

        //  RANDEVU EKLE MODEL
        
        public class RandevuEkleModel
        {
            public int AntrenorId { get; set; }
            public int HizmetId { get; set; }
            public DateTime Tarih { get; set; }
            public string Saat { get; set; }
        }

       
        //  RANDEVU OLUŞTUR
       
        [HttpPost("randevu-olustur")]
        public IActionResult RandevuOlustur([FromBody] RandevuEkleModel model)
        {
            var uyeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uyeId))
                return Unauthorized(new { mesaj = "Lütfen önce giriş yapınız." });

            // Geçmiş tarih engeli
            if (model.Tarih.Date < DateTime.Today)
                return BadRequest("Geçmiş tarihe randevu alınamaz.");

            if (model.Tarih.Date == DateTime.Today)
            {
                var secilenSaat = TimeSpan.Parse(model.Saat);
                if (secilenSaat <= DateTime.Now.TimeOfDay)
                    return BadRequest("Geçmiş saate randevu alınamaz.");
            }

            var hizmet = _context.Hizmetler.Find(model.HizmetId);
            if (hizmet == null)
                return BadRequest("Hizmet bulunamadı.");

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

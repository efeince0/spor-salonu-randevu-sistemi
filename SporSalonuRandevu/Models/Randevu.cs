using System;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuRandevu.Models
{
    public enum RandevuDurumu
    {
        Beklemede = 0,
        Onaylandi = 1,
        IptalEdildi = 2
    }
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; } = null!;

        [Required]
        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; } = null!;

        [Required]
        public string UyeId { get; set; } = null!;
        public Uye Uye { get; set; } = null!;

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Saat zorunludur.")]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Saat formatı HH:mm olmalıdır.")]
        public string Saat { get; set; } = null!;

        [Required]
        public RandevuDurumu Durum { get; set; }

     
    }
}

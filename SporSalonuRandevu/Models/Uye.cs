using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuRandevu.Models
{
    public class Uye : IdentityUser
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(60, ErrorMessage = "Ad Soyad en fazla 60 karakter olabilir.")]
        public string AdSoyad { get; set; } = null!;

        // opsiyonel alanlar

        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır.")]
        public int? Yas { get; set; }

        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır.")]
        public double? Boy { get; set; }

        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır.")]
        public double? Kilo { get; set; }
    }
}

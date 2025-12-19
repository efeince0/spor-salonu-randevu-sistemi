using System.ComponentModel.DataAnnotations;

namespace SporSalonuRandevu.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Ad Soyad 3-100 karakter arasında olmalıdır.")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Uzmanlık en fazla 100 karakter olabilir.")]
        public string Uzmanlik { get; set; }

        [Required(ErrorMessage = "Çalışma başlangıç saati zorunludur.")]
        public TimeSpan CalismaBaslangic { get; set; }

        [Required(ErrorMessage = "Çalışma bitiş saati zorunludur.")]
        public TimeSpan CalismaBitis { get; set; }
    }
}

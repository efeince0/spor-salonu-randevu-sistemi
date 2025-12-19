using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuRandevu.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir.")]
        public string Ad { get; set; } = null!;

        [Required(ErrorMessage = "Hizmet süresi zorunludur.")]
        [Range(15, 300, ErrorMessage = "Hizmet süresi 15–300 dakika arasında olmalıdır.")]
        public int SureDakika { get; set; }

        [Required(ErrorMessage = "Hizmet ücreti zorunludur.")]
        [Range(0, 100000, ErrorMessage = "Hizmet ücreti geçerli bir değer olmalıdır.")]
        public decimal Ucret { get; set; }

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}

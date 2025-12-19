using System;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuRandevu.Models
{
    public class Bildirim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Üye bilgisi zorunludur.")]
        public string UyeId { get; set; } = null!;

        public Uye Uye { get; set; } = null!;

        [Required(ErrorMessage = "Bildirim mesajı boş bırakılamaz.")]
        [StringLength(300, ErrorMessage = "Bildirim mesajı en fazla 300 karakter olabilir.")]
        public string Mesaj { get; set; } = null!;

        public bool OkunduMu { get; set; } = false;

        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}

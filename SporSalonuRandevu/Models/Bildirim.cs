using System;

namespace SporSalonuRandevu.Models
{
    public class Bildirim
    {
        public int Id { get; set; }

        public string UyeId { get; set; } = null!;
        public Uye Uye { get; set; } = null!;

        public string Mesaj { get; set; } = null!;

        public bool OkunduMu { get; set; } = false;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}

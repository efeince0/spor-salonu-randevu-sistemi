namespace SporSalonuRandevu.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        public DateTime Tarih { get; set; }
        public string Saat { get; set; } = null!;

        public string UyeId { get; set; } = null!;
        public Uye Uye { get; set; } = null!;

        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; } = null!;

        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; } = null!;
    }
}

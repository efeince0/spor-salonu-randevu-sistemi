namespace SporSalonuRandevu.Models
{
    public class Antrenor
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = null!;
        public string? Uzmanlik { get; set; }

        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

    }
}

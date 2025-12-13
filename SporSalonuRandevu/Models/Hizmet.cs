namespace SporSalonuRandevu.Models
{
    public class Hizmet
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public int SureDakika { get; set; }
        public decimal Ucret { get; set; }

        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        public ICollection<Randevu>? Randevular { get; set; }
    }
}

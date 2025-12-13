namespace SporSalonuRandevu.Models
{
    public class Salon
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string? Adres { get; set; }

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}

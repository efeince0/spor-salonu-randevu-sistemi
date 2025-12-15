namespace SporSalonuRandevu.Models
{
    public class Hizmet
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public int SureDakika { get; set; }
        public decimal Ucret { get; set; }
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

    }
}

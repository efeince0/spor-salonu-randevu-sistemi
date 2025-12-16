namespace SporSalonuRandevu.Models
{
    public class Antrenor
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Uzmanlik { get; set; }

        public TimeSpan CalismaBaslangic { get; set; }
        public TimeSpan CalismaBitis { get; set; }
    }


}
    
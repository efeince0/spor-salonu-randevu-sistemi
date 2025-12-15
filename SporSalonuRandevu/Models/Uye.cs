using Microsoft.AspNetCore.Identity;
namespace SporSalonuRandevu.Models


{
    public class Uye : IdentityUser
    {
        public string? AdSoyad { get; set; }
        public int? Yas { get; set; }
        public int? Boy { get; set; }
        public int? Kilo { get; set; }
    }
}

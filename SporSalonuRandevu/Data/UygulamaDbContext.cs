using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Models;


namespace SporSalonuRandevu.Data
{
    public class UygulamaDbContext : IdentityDbContext<Uye>
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
            : base(options) { }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }

}

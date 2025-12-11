using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTeknolojileriProje.Models;



namespace SporSalonuRandevu.Data

{
    public class UygulamaDbContext : IdentityDbContext<Uye>
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
            : base(options)
        {
        }
    }
}

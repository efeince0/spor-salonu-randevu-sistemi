using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SporSalonuRandevu.Data;
using SporSalonuRandevu.Models;
using SporSalonuRandevu.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ===============================
// DATABASE (PostgreSQL)
// ===============================
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===============================
// IDENTITY
// ===============================
builder.Services.AddIdentity<Uye, IdentityRole>(options =>
{
    // 🔥 ŞİFRE KURALLARI (Geliştirme ortamı için basitleştirilmiş)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UygulamaDbContext>()
.AddDefaultTokenProviders();

// ===============================
// MVC & SESSION (BURASI EKLENDİ)
// ===============================
builder.Services.AddControllersWithViews();

// 🟢 1. SESSION SERVİSİ EKLENDİ (Diyet listesi hafızada kalsın diye)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika hafızada tutar
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🟢 2. HTTP CLIENT TIMEOUT AYARI (Uzun süren AI cevapları için)
builder.Services.AddHttpClient<GeminiService>(client =>
{
    // Varsayılan 100 saniyedir, AI bazen uzun düşünür, bunu 5 dk yaptık.
    client.Timeout = TimeSpan.FromMinutes(5);
});


var app = builder.Build();

// ===============================
// MIDDLEWARE
// ===============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🟢 3. SESSION MIDDLEWARE EKLENDİ (Authentication'dan önce olmalı)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ===============================
// ROUTE
// ===============================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===============================
// ROL + ADMIN SEED
// ===============================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Uye>>();

    // Roller
    string[] roller = { "Admin", "Uye" };

    foreach (var rol in roller)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // Admin hesabı
    string adminEmail = "g231210011@ogr.sakarya.edu.tr";
    string adminPassword = "sau";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var admin = new Uye
        {
            UserName = adminEmail,
            Email = adminEmail,
            AdSoyad = "Admin"
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();
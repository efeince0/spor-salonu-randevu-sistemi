using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalonuRandevu.Models;

namespace SporSalonuRandevu.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Uye> _userManager;
        private readonly SignInManager<Uye> _signInManager;

        public AccountController(
            UserManager<Uye> userManager,
            SignInManager<Uye> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // LOGIN GET
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(
                email, password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);

                // Admin mi kontrolü
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin"); // Admin paneline yönlendirme
                }

                return RedirectToAction("Index", "Home"); // Normal kullanıcıyı anasayfaya yönlendir
            }

            ViewBag.Hata = "Giriş başarısız";
            return View();
        }


        // REGISTER GET
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER POST
        [HttpPost]
        public async Task<IActionResult> Register(string adSoyad, string email, string password)
        {
            var uye = new Uye
            {
                UserName = email,
                Email = email,
                AdSoyad = adSoyad
            };

            var result = await _userManager.CreateAsync(uye, password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(uye, false);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hata = "Kayıt başarısız";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}

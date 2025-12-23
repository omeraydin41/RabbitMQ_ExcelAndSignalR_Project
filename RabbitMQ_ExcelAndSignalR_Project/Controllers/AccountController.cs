using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RabbitMQ_ExcelAndSignalR_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        // Kullanıcıyı bulma, oluşturma, şifre doğrulama gibi user işlemlerini yapmak için

        private readonly SignInManager<IdentityUser> _signInManager;
        // Kullanıcının giriş (login) ve çıkış (logout) işlemlerini yönetmek için

        public AccountController(
            UserManager<IdentityUser> userManager,      // Identity tarafından sağlanan kullanıcı yönetim servisi
            SignInManager<IdentityUser> signInManager)  // Identity tarafından sağlanan oturum yönetim servisi
        {
            _userManager = userManager;     // UserManager DI ile alınır ve class içinde kullanılır
            _signInManager = signInManager; // SignInManager DI ile alınır ve login işlemlerinde kullanılır
        }

        public IActionResult Login()//logın ekranıdır 
        {
            return View();
        }

        [HttpPost]// kullanıcının formdan gönderdiği verileri (email, şifre gibi) sunucuya güvenli şekilde göndermek için
        public async Task<IActionResult> Login(string Email, string Password) // Login formu POST edildiğinde çalışır
        {
            var hasUser = await _userManager.FindByEmailAsync(Email); // Girilen email ile kullanıcıyı bul

            if (hasUser == null) // Kullanıcı bulunamazsa
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı"); // Hata mesajı ekle
                return View(); // Login ekranına geri dön
            }

            var signInResult = await _signInManager.PasswordSignInAsync( // Şifre ile giriş denemesi yap
                hasUser,        // Giriş yapacak kullanıcı
                Password,       // Formdan gelen şifre
                true,           // Beni hatırla (kalıcı cookie)
                false           // Yanlış denemede kullanıcıyı kilitleme
            );

            if (!signInResult.Succeeded) // Giriş başarısızsa
            {
                return View(); // Login ekranını tekrar göster
            }

            return RedirectToAction(nameof(HomeController.Index), "home");
            // Login başarılı olduktan sonra HomeController içindeki Index action'ına yönlendirir

        }
    }

}

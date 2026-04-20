namespace WebCMS.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;

    public class AccountController : Controller
    {
        // ── 1. Trang Login ─────────────────────────────
        public IActionResult Login()
        {
            return View();
        }

        // ── 2. Xử lý Login ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 🔴 Check rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                ViewBag.Username = username;
                return View();
            }

            string role = "";
            string userId = "";

            // 🔥 DEMO LOGIN (map đúng DB)
            if (username == "admin" && password == "123456")
            {
                role = "admin";
                userId = "U001";
            }
            else if (username == "owner1" && password == "123456")
            {
                role = "owner";
                userId = "U002";
            }
            else if (username == "owner2" && password == "123456")
            {
                role = "owner";
                userId = "U005";
            }
            else
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
                ViewBag.Username = username; // ✅ giữ lại input
                return View();
            }

            // 🔴 XÓA COOKIE CŨ (tránh bug session)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 🔴 CLAIM CHUẨN
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId), // 🔥 QUAN TRỌNG
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 🔴 LOGIN
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // 🔥 Redirect chung (không cần if nữa)
            return RedirectToAction("Index", "POI");
        }

        // ── 3. Logout ─────────────────────────────
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
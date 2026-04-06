namespace WebCMS.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class AccountController : Controller
    {
        // 1. Trang hiện giao diện Login
        public IActionResult Login()
        {
            return View();
        }

        // 2. Xử lý khi nhấn nút "Sign In"
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Kiểm tra tài khoản đơn giản (Sau này bạn có thể lấy từ Database)
            if (username == "admin" && password == "123456")
            {
                // Nếu đúng, chuyển hướng vào trang danh sách POI
                return RedirectToAction("Index", "POI");
            }

            // Nếu sai, báo lỗi và ở lại trang Login
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}

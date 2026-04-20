using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebCMS.Controllers
{
    // 🔥 Bắt buộc đăng nhập mới được vào
    [Authorize(Roles = "admin,owner")]
    public class QRCodeController : Controller
    {
        private readonly IConfiguration _configuration;

        public QRCodeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.ApiUrl = string.IsNullOrEmpty(apiUrl) ? "https://gzm4vrwg-7054.asse.devtunnels.ms/api/" : apiUrl;

            // 🔥 Lấy thông tin từ Cookie (Claims), xóa sạch rác của Session cũ
            ViewBag.CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.UserRole = User.IsInRole("admin") ? "admin" : "owner";

            return View();
        }

        public IActionResult Create()
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.ApiUrl = string.IsNullOrEmpty(apiUrl) ? "https://gzm4vrwg-7054.asse.devtunnels.ms/api/" : apiUrl;

            // 🔥 Lấy thông tin từ Cookie (Claims)
            ViewBag.CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.UserRole = User.IsInRole("admin") ? "admin" : "owner";

            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebCMS.Controllers
{
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

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? HttpContext.Session.GetString("UserID");
            var userRole = User.IsInRole("owner") ? "owner" : HttpContext.Session.GetString("Role");

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.UserRole = userRole;

            return View();
        }
        public IActionResult Create()
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.ApiUrl = string.IsNullOrEmpty(apiUrl) ? "https://gzm4vrwg-7054.asse.devtunnels.ms/api/" : apiUrl;

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? HttpContext.Session.GetString("UserID");
            var userRole = User.IsInRole("owner") ? "owner" : HttpContext.Session.GetString("Role");

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.UserRole = userRole;

            return View();
        }
    }
}
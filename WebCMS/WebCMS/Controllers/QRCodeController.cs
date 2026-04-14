using Microsoft.AspNetCore.Mvc;

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

            //  Kiểm tra nếu chưa cấu hình thì báo lỗi hoặc gán mặc định để tránh crash JS
            if (string.IsNullOrEmpty(apiUrl))
            {
                ViewBag.ApiUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api/"; // Giá trị dự phòng
            }
            else
            {
                ViewBag.ApiUrl = apiUrl;
            }

            return View();
        }
        public IActionResult Create()
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.ApiUrl = string.IsNullOrEmpty(apiUrl) ? "https://gzm4vrwg-7054.asse.devtunnels.ms/api/" : apiUrl;
            return View();
        }
    }
}
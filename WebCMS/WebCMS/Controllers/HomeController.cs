using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebCMS.Models;

namespace WebCMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _apiUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api/Stats";

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();
            try
            {
                using var client = new HttpClient();

                // Gửi Request lấy dữ liệu từ Server API
                var response = await client.GetAsync($"{_apiUrl}/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // Chuyển JSON thành Object Model
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    model = JsonSerializer.Deserialize<DashboardViewModel>(jsonString, options) ?? new DashboardViewModel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy dữ liệu Dashboard: " + ex.Message);
            }

            return View(model);
        }

        // Đẩy thẳng kết quả API về cho Javascript vẽ số Real-time
        [HttpGet]
        public async Task<IActionResult> GetRealTimeActiveUsers()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"{_apiUrl}/active-users");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return Content(jsonString, "application/json");
                }
            }
            catch { }

            return Json(new { count = 0 });
        }
    }
}
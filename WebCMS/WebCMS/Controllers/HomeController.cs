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
                var response = await client.GetAsync($"{_apiUrl}/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
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

        // 🔥 HÀM MỚI: Phục vụ bộ lọc chọn thời gian 7, 14, 21, 30 ngày từ AJAX (Javascript)
        [HttpGet]
        public async Task<IActionResult> GetTopPois(int days)
        {
            try
            {
                using var client = new HttpClient();

                // Gọi tới hàm API mới bên Server
                var response = await client.GetAsync($"{_apiUrl}/top-pois?days={days}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    // Trả thẳng chuỗi JSON lấy được từ API về cho Javascript để vẽ biểu đồ
                    return Content(jsonString, "application/json");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lọc biểu đồ: " + ex.Message);
            }

            return Json(new { labels = new List<string>(), values = new List<int>() });
        }
    }
}
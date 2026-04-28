using System.Net.Http.Json;
using WebCMS.Models;

namespace WebCMS.Services
{
    public class StatsService
    {
        private readonly HttpClient _http;
        public StatsService(HttpClient http) => _http = http;

        public async Task<DashboardViewModel> GetFullDashboardAsync()
        {
            try
            {
                // Gọi thẳng tới endpoint API mới đã gộp chung tất cả dữ liệu
                // Hàm GetFromJsonAsync tự động map JSON vào DashboardViewModel (bỏ qua khoảng trắng/viết hoa chữ thường)
                return await _http.GetFromJsonAsync<DashboardViewModel>("Stats/dashboard")
                       ?? new DashboardViewModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy dữ liệu StatsService: " + ex.Message);
                return new DashboardViewModel();
            }
        }
    }
}
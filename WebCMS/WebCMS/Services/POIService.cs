using System.Net.Http.Json; // Cần thiết để dùng GetFromJsonAsync và PostAsJsonAsync
using WebCMS.Models;

namespace WebCMS.Services
{
    public class POIService : IPOIService
    {
        private readonly HttpClient _http;

        // HttpClient đã được cấu hình BaseAddress từ Program.cs nên không cần viết lại ở đây
        public POIService(HttpClient http)
        {
            _http = http;
        }

        // Chuyển tất cả sang Task (Bất đồng bộ - Async/Await)
        public async Task<List<POI>> GetAllAsync()
        {
            // Gọi API GET và tự động chuyển JSON thành List<POI>
            var response = await _http.GetFromJsonAsync<List<POI>>("POI");
            return response ?? new List<POI>();
        }

        public async Task<POI?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<POI>($"POI/{id}");
        }

        public async Task<bool> CreateAsync(POI poi)
        {
            var response = await _http.PostAsJsonAsync("POI", poi);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(POI poi)
        {
            var response = await _http.PutAsJsonAsync($"POI/{poi.Id}", poi);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"POI/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
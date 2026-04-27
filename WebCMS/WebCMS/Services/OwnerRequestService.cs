using WebCMS.Models;
using System.Net.Http.Json;

namespace WebCMS.Services
{
    public class OwnerRequestService
    {
        private readonly HttpClient _http;

        public OwnerRequestService(HttpClient http)
        {
            _http = http;
        }

        // 🟢 LẤY DANH SÁCH YÊU CẦU
        public async Task<List<OwnerRequest>> GetAllAsync()
        {
            // Bỏ chữ "api/" đi vì BaseAddress đã có sẵn
            return await _http.GetFromJsonAsync<List<OwnerRequest>>("OwnerRequest")
                   ?? new List<OwnerRequest>();
        }

        // 🟡 DUYỆT YÊU CẦU
        public async Task<bool> ApproveAsync(string id)
        {
            // Bỏ chữ "api/" đi vì BaseAddress đã có sẵn
            var response = await _http.PostAsync($"OwnerRequest/Approve/{id}", null);
            return response.IsSuccessStatusCode;
        }

        // 🔴 TỪ CHỐI YÊU CẦU
        public async Task<bool> RejectAsync(string id)
        {
            // Bỏ chữ "api/" đi vì BaseAddress đã có sẵn
            var response = await _http.PostAsync($"OwnerRequest/Reject/{id}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
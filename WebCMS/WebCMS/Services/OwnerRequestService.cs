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

        public async Task<List<OwnerRequest>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<OwnerRequest>>("OwnerRequest")
                   ?? new List<OwnerRequest>();
        }

        // Trong file OwnerRequestService.cs
        public async Task<bool> ApproveAsync(string id) // Đổi int -> string
        {
            var response = await _http.PostAsync($"api/OwnerRequest/Approve/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectAsync(string id) // Đổi int -> string
        {
            var response = await _http.PostAsync($"api/OwnerRequest/Reject/{id}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
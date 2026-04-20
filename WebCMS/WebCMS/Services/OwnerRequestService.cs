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

        public async Task ApproveAsync(int id)
        {
            await _http.PostAsync($"OwnerRequest/Approve/{id}", null);
        }

        public async Task RejectAsync(int id)
        {
            await _http.PostAsync($"OwnerRequest/Reject/{id}", null);
        }
    }
}
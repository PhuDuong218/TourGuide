using System.Net.Http.Json;
using WebCMS.Models;

namespace WebCMS.Services
{
    public class VisitHistoryService : IVisitHistoryService
    {
        private readonly HttpClient _http;

        public VisitHistoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<VisitHistory>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<VisitHistory>>("api/VisitHistory");
            return result ?? new List<VisitHistory>();
        }
    }
}
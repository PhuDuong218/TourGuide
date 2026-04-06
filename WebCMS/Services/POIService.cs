using System.Net.Http.Json;
using WebCMS.Models;

namespace WebCMS.Services
{
    public class POIService : IPOIService
    {
        private readonly HttpClient _http;

        public POIService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<POI>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<POI>>("POI") ?? new List<POI>();
        }

        public async Task<POI?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<POI>($"POI/{id}");
        }

        public async Task CreateAsync(POI poi)
        {
            await _http.PostAsJsonAsync("POI", poi);
        }

        public async Task DeleteAsync(string id)
        {
            await _http.DeleteAsync($"POI/{id}");
        }
    }
}
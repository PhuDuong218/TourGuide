using WebCMS.Models;
using System.Net.Http.Json;

namespace WebCMS.Services
{
    public class TranslationService
    {
        private readonly HttpClient _http;

        public TranslationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<POITranslation>> GetByPOIAsync(string id)
        {
            return await _http.GetFromJsonAsync<List<POITranslation>>($"Translation/{id}")
                   ?? new List<POITranslation>();
        }

        public async Task CreateAsync(POITranslation t)
        {
            await _http.PostAsJsonAsync("Translation", t);
        }

        public async Task DeleteAsync(string poiId, string lang)
        {
            await _http.DeleteAsync($"Translation/{poiId}/{lang}");
        }
    }
}
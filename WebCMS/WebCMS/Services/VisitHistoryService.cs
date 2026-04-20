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
            try
            {
                var result = await _http.GetFromJsonAsync<List<VisitHistory>>("VisitHistory");
                return result ?? new List<VisitHistory>();
            }
            catch (Exception ex)
            {
                // Log debug (không crash app)
                System.Diagnostics.Debug.WriteLine("API VisitHistory error: " + ex.Message);
                return new List<VisitHistory>();
            }
        }
    }
}
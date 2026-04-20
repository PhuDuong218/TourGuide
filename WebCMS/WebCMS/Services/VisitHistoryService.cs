using WebCMS.Models;
using System.Net.Http.Json;

namespace WebCMS.Services
{
    public class VisitHistoryService
    {
        private readonly HttpClient _http;
        public VisitHistoryService(HttpClient http) => _http = http;

        public async Task<List<VisitHistory>> GetAllAsync()
        {
            try
            {
                // Đảm bảo "VisitHistory" khớp với tên Controller bên Server
                var response = await _http.GetFromJsonAsync<List<VisitHistory>>("VisitHistory");
                return response ?? new List<VisitHistory>();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra cửa sổ Debug để dễ kiểm tra
                System.Diagnostics.Debug.WriteLine("API Error: " + ex.Message);
                return new List<VisitHistory>(); 
            }
        }
    }
}
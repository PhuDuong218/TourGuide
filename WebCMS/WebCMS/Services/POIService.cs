using System.Net.Http;
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
            var response = await _http.GetAsync($"poi/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<POI>();
        }

        public async Task UpdateAsync(string id, POI poi, IFormFile? imageFile)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(id), "POIID");

            if (!string.IsNullOrEmpty(poi.Category))
            {
                content.Add(new StringContent(poi.Category), "Category");
            }

            content.Add(new StringContent(poi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Latitude");
            content.Add(new StringContent(poi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Longitude");

            if (!string.IsNullOrEmpty(poi.Img))
            {
                content.Add(new StringContent(poi.Img), "Img");
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                content.Add(streamContent, "imageFile", imageFile.FileName);
            }

            var response = await _http.PutAsync($"POI/{id}", content);
            response.EnsureSuccessStatusCode(); // Nếu Server trả về lỗi, dòng này sẽ ném ra Exception và hiện lên giao diện ở Bước 1
        }

        public async Task CreateAsync(POI poi, IFormFile? imageFile)
        {
            using var content = new MultipartFormDataContent();

            // Tự động sinh ID ngẫu nhiên nếu form không có trường nhập ID (tránh lỗi Primary Key trong SQL)
            // Tự động sinh ID ngẫu nhiên (Ví dụ: P1234)
            if (string.IsNullOrEmpty(poi.POIID))
            {
                Random rnd = new Random();
                poi.POIID = "P" + rnd.Next(1000, 9999).ToString(); // Kết quả ra 5 ký tự, VD: P8472
            }
            content.Add(new StringContent(poi.POIID), "POIID");

            if (!string.IsNullOrEmpty(poi.Name)) content.Add(new StringContent(poi.Name), "RestaurantName");
            if (!string.IsNullOrEmpty(poi.Category)) content.Add(new StringContent(poi.Category), "Category");
            if (!string.IsNullOrEmpty(poi.Description)) content.Add(new StringContent(poi.Description ), "Address"); 

            // Ép tọa độ dùng dấu chấm chuẩn quốc tế
            content.Add(new StringContent(poi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Latitude");
            content.Add(new StringContent(poi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Longitude");

            // Đóng gói file ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                // Tên "imageFile" ở đây phải trùng khớp hoàn toàn với tham số trong API Server
                content.Add(streamContent, "imageFile", imageFile.FileName);
            }

            var response = await _http.PostAsync("POI", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string id)
        {
            await _http.DeleteAsync($"POI/{id}");
        }
    }
}
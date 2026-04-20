using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using WebCMS.Models;
using Microsoft.AspNetCore.Http;

namespace WebCMS.Services
{
    public class POIService : IPOIService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public POIService(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        // 🔥 GẮN TOKEN
        private void AddToken()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ─────────────────────────────────────────────
        // 1. Lấy toàn bộ POI (việc lọc theo owner làm ở Controller)
        // ─────────────────────────────────────────────
        public async Task<List<POI>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<POI>>("POI") ?? new List<POI>();
        }

        // ─────────────────────────────────────────────
        // 2. Lấy POI theo ID
        // ─────────────────────────────────────────────
        public async Task<POI?> GetByIdAsync(string id)
        {
            AddToken();

            var response = await _http.GetAsync($"POI/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<POI>();
        }

        // ─────────────────────────────────────────────
        // 3. Update POI
        // ─────────────────────────────────────────────
        public async Task UpdateAsync(string id, POI poi, IFormFile? imageFile)
        {
            AddToken();

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(id), "POIID");

            content.Add(new StringContent(poi.Priority.ToString()), "Priority");

            // Đảm bảo có gửi Tên và Địa chỉ
            if (!string.IsNullOrEmpty(poi.RestaurantName))
                content.Add(new StringContent(poi.RestaurantName), "RestaurantName");

            if (!string.IsNullOrEmpty(poi.Address))
                content.Add(new StringContent(poi.Address), "Address");

            if (!string.IsNullOrEmpty(poi.Category))
                content.Add(new StringContent(poi.Category), "Category");

            content.Add(new StringContent(poi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Latitude");
            content.Add(new StringContent(poi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Longitude");

            // ✅ FIX: dùng poi.Img thay vì poi.Image
            if (!string.IsNullOrEmpty(poi.Img))
                content.Add(new StringContent(poi.Img), "Img");

            if (imageFile != null && imageFile.Length > 0)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                content.Add(streamContent, "imageFile", imageFile.FileName);
            }

            var response = await _http.PutAsync($"POI/{id}", content);
            response.EnsureSuccessStatusCode();
        }

        // ─────────────────────────────────────────────
        // 4. Create POI
        // ─────────────────────────────────────────────
        public async Task CreateAsync(POI poi, IFormFile? imageFile)
        {
            AddToken();

            using var content = new MultipartFormDataContent();

            // ✅ FIX: dùng poi.Id thay vì poi.POIID
            if (string.IsNullOrEmpty(poi.POIID))
            {
                Random rnd = new Random();
                poi.POIID = "P" + rnd.Next(1000, 9999);
            }

            content.Add(new StringContent(poi.POIID), "POIID");

            if (!string.IsNullOrEmpty(poi.RestaurantName))
                content.Add(new StringContent(poi.RestaurantName), "RestaurantName");

            if (!string.IsNullOrEmpty(poi.Category))
                content.Add(new StringContent(poi.Category), "Category");

            if (!string.IsNullOrEmpty(poi.RestaurantName)) content.Add(new StringContent(poi.RestaurantName), "RestaurantName");
            if (!string.IsNullOrEmpty(poi.Category)) content.Add(new StringContent(poi.Category), "Category");
            if (!string.IsNullOrEmpty(poi.Address)) content.Add(new StringContent(poi.Address ), "Address"); 

            content.Add(new StringContent(poi.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Latitude");
            content.Add(new StringContent(poi.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Longitude");

            if (!string.IsNullOrEmpty(poi.OwnerID))
                content.Add(new StringContent(poi.OwnerID), "OwnerID");

            if (imageFile != null && imageFile.Length > 0)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                content.Add(streamContent, "imageFile", imageFile.FileName);
            }

            var response = await _http.PostAsync("POI", content);
            response.EnsureSuccessStatusCode();
        }

        // ─────────────────────────────────────────────
        // 5. Delete
        // ─────────────────────────────────────────────
        public async Task DeleteAsync(string id)
        {
            AddToken();

            var response = await _http.DeleteAsync($"POI/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
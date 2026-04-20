using System.Net.Http.Json;
using System.Globalization;
using TourGuideMauiApp.Models;
using Microsoft.Maui.Storage; // Thêm thư viện này để dùng Preferences

namespace TourGuideMauiApp.Services;

public class DatabaseService
{
    private readonly HttpClient _httpClient;

    // SỬA Ở ĐÂY: Biến _lang thành Property động, luôn lấy ngôn ngữ mới nhất từ cài đặt
    // Nếu người dùng chưa cài đặt, mặc định sẽ là "vi" (Tiếng Việt)
    private string _lang => Preferences.Get("AppLanguage", "vi");

    public DatabaseService()
    {
        _httpClient = new HttpClient
        {
            // Link Dev Tunnel của bạn (Giữ nguyên)
            BaseAddress = new Uri("https://gzm4vrwg-7054.asse.devtunnels.ms/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
        // ĐÃ XÓA dòng gán _lang cố định ở đây
    }

    public async Task<List<POIDTO>> GetPointsOfInterestAsync()
    {
        try
        {
            // Bây giờ nó sẽ tự động lấy _lang mới nhất (vi, en, fr, ja...)
            var result = await _httpClient.GetFromJsonAsync<List<POIDTO>>($"api/POI?lang={_lang}");
            return result ?? new List<POIDTO>();
        }
        catch { return new List<POIDTO>(); }
    }

    public async Task<POIDTO?> GetPOIByIdAsync(string id, string? lang = null)
    {
        try
        {
            var targetLang = lang ?? _lang;
            return await _httpClient.GetFromJsonAsync<POIDTO>($"api/POI/{id}?lang={targetLang}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] GetById lỗi: {ex.Message}");
            return null;
        }
    }

    public async Task<POIDTO?> GetPOIByQRAsync(string qrValue)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<POIDTO>($"api/POI/qr/{Uri.EscapeDataString(qrValue)}?lang={_lang}");
            return result;
        }
        catch { return null; }
    }
    public async Task<bool> SendOwnerRequestAsync(OwnerRequestDTO request)
    {
        try
        {
            // Endpoint này chúng ta sẽ viết ở phía Web Admin sau
            var response = await _httpClient.PostAsJsonAsync("api/OwnerRequest", request);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
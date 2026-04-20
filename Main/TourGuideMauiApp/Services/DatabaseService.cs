using System.Net.Http.Json;
using System.Globalization;
using TourGuideMauiApp.Models;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace TourGuideMauiApp.Services;

public class DatabaseService
{
    private readonly HttpClient _httpClient;

    // Lấy ngôn ngữ động từ Preferences
    private string _lang => Preferences.Get("AppLanguage", "vi");

    public DatabaseService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://gzm4vrwg-7054.asse.devtunnels.ms/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    // 1. Lấy danh sách POI
    public async Task<List<POIDTO>> GetPointsOfInterestAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<POIDTO>>($"api/POI?lang={_lang}");
            return result ?? new List<POIDTO>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@$"[DB] GetPOIs Error: {ex.Message}");
            return new List<POIDTO>();
        }
    }

    // 2. Lấy chi tiết POI theo ID
    public async Task<POIDTO?> GetPOIByIdAsync(string id, string? lang = null)
    {
        try
        {
            var targetLang = lang ?? _lang;
            return await _httpClient.GetFromJsonAsync<POIDTO>($@"api/POI/{id}?lang={targetLang}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@$"[DB] GetById Error: {ex.Message}");
            return null;
        }
    }

    // 3. Lấy POI qua mã QR
    public async Task<POIDTO?> GetPOIByQRAsync(string qrValue)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<POIDTO>($@"api/POI/qr/{Uri.EscapeDataString(qrValue)}?lang={_lang}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@$"[DB] GetByQR Error: {ex.Message}");
            return null;
        }
    }

    // 4. 🔥 GỬI YÊU CẦU ĐĂNG KÝ ĐỐI TÁC
    public async Task<bool> SendOwnerRequestAsync(OwnerRequestDTO request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/OwnerRequest", request);

            if (response.IsSuccessStatusCode) return true;

            // 🔥 NẾU LỖI, HÃY ĐỌC NỘI DUNG LỖI TẠI ĐÂY
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[LỖI SERVER]: {response.StatusCode} - {errorContent}");

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LỖI KẾT NỐI]: {ex.Message}");
            return false;
        }
    }
}
using System.Net.Http.Json;
using System.Globalization;
using TourGuideMauiApp.Models;

namespace TourGuideMauiApp.Services;

public class DatabaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _lang;

    public DatabaseService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://gzm4vrwg-7054.asse.devtunnels.ms/api"),
            Timeout = TimeSpan.FromSeconds(15)
        };
        _lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public async Task<List<POIDTO>> GetPointsOfInterestAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<POIDTO>>($"api/POI?lang={_lang}");
            return result ?? new List<POIDTO>();
        }
        catch { return new List<POIDTO>(); }
    }

    public async Task<POIDTO?> GetPOIByIdAsync(string id, string? lang = null)
{
    try
    {
        // Nếu không truyền lang, dùng mặc định của hệ thống
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
}
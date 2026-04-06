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
        // ⚠️ Đổi IP này thành IP máy tính chạy server
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://192.168.1.144:5015/"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        // Lấy ngôn ngữ thiết bị: "vi", "en", ...
        _lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    // ── Lấy tất cả POI ───────────────────────────────────────────────────────
    public async Task<List<POIDTO>> GetPointsOfInterestAsync()
    {
        try
        {
            var result = await _httpClient
                .GetFromJsonAsync<List<POIDTO>>($"api/POI?lang={_lang}");
            return result ?? new List<POIDTO>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] GetAll lỗi: {ex.Message}");
            return new List<POIDTO>();
        }
    }

    // ── Lấy 1 POI theo ID ────────────────────────────────────────────────────
    public async Task<POIDTO?> GetPOIByIdAsync(int id)
    {
        try
        {
            return await _httpClient
                .GetFromJsonAsync<POIDTO>($"api/POI/{id}?lang={_lang}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] GetById lỗi: {ex.Message}");
            return null;
        }
    }

    // ── Lấy POI theo mã QR ───────────────────────────────────────────────────
    // Endpoint: GET /api/POI/qr/{qrValue}?lang=vi
    public async Task<POIDTO?> GetPOIByQRAsync(string qrValue)
    {
        try
        {
            return await _httpClient
                .GetFromJsonAsync<POIDTO>(
                    $"api/POI/qr/{Uri.EscapeDataString(qrValue)}?lang={_lang}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // QR không tồn tại trong DB → trả null thay vì throw
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] GetByQR lỗi: {ex.Message}");
            return null;
        }
    }
}
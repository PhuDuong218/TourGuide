using Mapsui.Providers.Wms;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;
using ZXing.Net.Maui;
using System.Text;
using System.Text.Json;

namespace TourGuideMauiApp.Views;

public partial class ScannerPage : ContentPage
{
    private readonly DatabaseService _dbService = new();
    private readonly TTSService _ttsService = new();

    private bool _isProcessing = false;
    private POIDTO? _currentPOI = null;

    public ScannerPage()
    {
        InitializeComponent();
        StartScanLineAnimation();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        barcodeReader.IsDetecting = true;
        _isProcessing = false;
        resultFrame.IsVisible = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //barcodeReader.IsDetecting = false;  // Tắt camera tiết kiệm pin
    }

    // ── Quét được mã QR ──────────────────────────────────────────────────────
    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        barcodeReader.IsDetecting = false;

        var first = e.Results?.FirstOrDefault();
        if (first == null)
        {
            _isProcessing = false;
            barcodeReader.IsDetecting = true;
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() => HandleQRCode(first.Value));
    }

    private async Task HandleQRCode(string qrValue)
    {
        scanLoading.IsRunning = true;
        scanLoading.IsVisible = true;
        resultFrame.IsVisible = false;

        try
        {
            var found = await _dbService.GetPOIByQRAsync(qrValue);

            if (found == null)
            {
                await DisplayAlert("Không tìm thấy",
                    $"Mã QR '{qrValue}' không khớp với địa điểm nào trong hệ thống.", "OK");
                barcodeReader.IsDetecting = true;
                _isProcessing = false;
                return;
            }

            _currentPOI = found;
            resultTitle.Text = found.RestaurantName;
            resultDescription.Text = string.IsNullOrEmpty(found.Address)
                ? found.ShortDescription
                : $"{found.ShortDescription}\n📍 {found.Address}";
            resultFrame.IsVisible = true;

            // --- ĐOẠN CODE GỬI LỊCH SỬ ĐÃ ĐƯỢC ĐẶT ĐÚNG CHỖ ---
            try
            {
                using var client = new HttpClient();
                string? currentUserId = Preferences.Get("LoggedInUserId", null);
                var historyData = new
                {
                    POIID = found.POIID,
                    UserID = currentUserId,
                    ScanMethod = "QR_Scan",
                    UserLat = found.Latitude,
                    UserLon = found.Longitude
                };

                string jsonString = System.Text.Json.JsonSerializer.Serialize(historyData);
                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                await client.PostAsync("https://gzm4vrwg-7054.asse.devtunnels.ms/api/VisitHistory", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi lịch sử: " + ex.Message);
            }
            // ---------------------------------------------------

            // Tự động phát thuyết minh
            var narration = string.IsNullOrEmpty(found.NarrationText)
                ? found.ShortDescription : found.NarrationText;

            await _ttsService.SpeakAsync(narration);

            // Tự động chuyển qua Map
            await Task.Delay(1500);
            await Shell.Current.GoToAsync($"//MapPage?poiId={found.POIID}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể xử lý mã QR: {ex.Message}", "OK");
            barcodeReader.IsDetecting = true;
            _isProcessing = false;
        }
        finally
        {
            scanLoading.IsRunning = false;
            scanLoading.IsVisible = false;
        }
    }

    // ── Nút Nghe thuyết minh ─────────────────────────────────────────────────
    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        if (_currentPOI == null) return;

        var narration = string.IsNullOrEmpty(_currentPOI.NarrationText)
            ? _currentPOI.ShortDescription : _currentPOI.NarrationText;

        btnSpeak.IsEnabled = false;
        btnSpeak.Text = "🔊 Đang phát...";

        await _ttsService.SpeakAsync(narration);

        btnSpeak.IsEnabled = true;
        btnSpeak.Text = "🔊 Nghe thuyết minh";
        resultFrame.IsVisible = false;
        barcodeReader.IsDetecting = true;
        _isProcessing = false;
    }

    // ── Animation thanh quét ──────────────────────────────────────────────────
    private void StartScanLineAnimation() => _ = AnimateScanLine();

    private async Task AnimateScanLine()
    {
        while (true)
        {
            await scanLine.TranslateTo(0, 252, 1500);
            await scanLine.TranslateTo(0, 0, 1500);
        }
    }
}
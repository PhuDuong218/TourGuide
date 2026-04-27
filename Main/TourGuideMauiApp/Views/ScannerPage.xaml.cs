using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;
using ZXing.Net.Maui;

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

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;

        // Dừng nhận diện ngay để tránh quét trùng
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
                    $"Mã QR '{qrValue}' không khớp với địa điểm nào.", "OK");
                _isProcessing = false;
                barcodeReader.IsDetecting = true;
                return;
            }

            _currentPOI = found;
            resultTitle.Text = found.RestaurantName;
            resultDescription.Text = string.IsNullOrEmpty(found.Address)
                ? found.ShortDescription
                : $"{found.ShortDescription}\n📍 {found.Address}";
            resultFrame.IsVisible = true;

            // ✅ GỬI LỊCH SỬ QUA SERVICE (ĐÃ SỬA TỪ KHÓA "QR")
            _ = Task.Run(async () =>
            {
                string? currentUserId = Preferences.Get("LoggedInUserId", null);
                var historyData = new
                {
                    POIID = found.POIID,
                    UserID = currentUserId,
                    ScanMethod = "QR", // 🔥 Đổi từ "QR_Scan" thành "QR" để Dashboard đếm được
                    UserLat = found.Latitude,
                    UserLon = found.Longitude
                };
                await _dbService.SaveVisitHistoryAsync(historyData);
            });

            // Phát thuyết minh
            var narration = string.IsNullOrEmpty(found.NarrationText)
                ? found.ShortDescription : found.NarrationText;
            await _ttsService.SpeakAsync(narration);

            // Chuyển trang Map sau khi quét thành công
            await Task.Delay(1000);
            await Shell.Current.GoToAsync($"//MapPage?poiId={found.POIID}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Lỗi xử lý: {ex.Message}", "OK");
            _isProcessing = false;
            barcodeReader.IsDetecting = true;
        }
        finally
        {
            scanLoading.IsRunning = false;
            scanLoading.IsVisible = false;
        }
    }

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        if (_currentPOI == null) return;

        btnSpeak.IsEnabled = false;
        btnSpeak.Text = "🔊 Đang phát...";

        var narration = string.IsNullOrEmpty(_currentPOI.NarrationText)
            ? _currentPOI.ShortDescription : _currentPOI.NarrationText;

        await _ttsService.SpeakAsync(narration);

        btnSpeak.IsEnabled = true;
        btnSpeak.Text = "🔊 Nghe thuyết minh";
    }

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
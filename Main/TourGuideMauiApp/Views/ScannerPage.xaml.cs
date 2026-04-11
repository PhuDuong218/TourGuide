using ZXing.Net.Maui;
using TourGuideMauiApp.Services;
using TourGuideMauiApp.Models;

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
        barcodeReader.IsDetecting = false;  // Tắt camera tiết kiệm pin
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
            // Gọi đúng endpoint /api/POI/qr/{qrValue} — khớp với bảng QRCode trong DB
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
            resultTitle.Text = found.Name;
            resultDescription.Text = string.IsNullOrEmpty(found.Address)
                ? found.Description
                : $"{found.Description}\n📍 {found.Address}";
            resultFrame.IsVisible = true;

            // Tự động phát thuyết minh
            var narration = string.IsNullOrEmpty(found.Narration)
                ? found.Description : found.Narration;

            await _ttsService.SpeakAsync(narration);
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

        var narration = string.IsNullOrEmpty(_currentPOI.Narration)
            ? _currentPOI.Description : _currentPOI.Narration;

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
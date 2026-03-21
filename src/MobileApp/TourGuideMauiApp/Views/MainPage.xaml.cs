using Microsoft.Maui.Devices.Sensors;
using System.Globalization;

namespace TourGuideMauiApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Logic kiểm tra vị trí (PoC)
    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        try
        {
            // Tự động nhận diện ngôn ngữ
            string currentLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            // Lấy tọa độ GPS
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location != null)
            {
                LblLocation.Text = $"Tọa độ: {location.Latitude:F4}, {location.Longitude:F4}";

                // Tính khoảng cách Haversine đến điểm mẫu
                double targetLat = 10.7719;
                double targetLon = 106.6912;
                double distance = CalculateHaversine(location.Latitude, location.Longitude, targetLat, targetLon);

                LblStatus.Text = $"Ngôn ngữ: {currentLang.ToUpper()} | Cách quán: {distance * 1000:F0}m";

                if (distance < 0.05)
                {
                    await DisplayAlert("Thông báo", $"Đã đến điểm ẩm thực! Phát thuyết minh tiếng {currentLang}", "OK");
                }
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Lỗi", "Vui lòng bật GPS và cấp quyền định vị!", "OK");
        }
    }

    // Công thức Haversine
    private double CalculateHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        double r = 6371;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return r * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
    }

    // Quét QR (MVP)
    private async void OnScanQRClicked(object sender, EventArgs e)
    {
        await DisplayAlert("QR Code", "Chức năng quét mã đang được khởi tạo...", "OK");
    }
}
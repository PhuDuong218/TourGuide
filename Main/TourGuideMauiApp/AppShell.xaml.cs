using TourGuideMauiApp.Views;
using System.Net.Http;

namespace TourGuideMauiApp;

public partial class AppShell : Shell
{
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://gzm4vrwg-7054.asse.devtunnels.ms/") };
    private IDispatcherTimer? _heartbeatTimer;

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(RegisterOwnerPage), typeof(RegisterOwnerPage));

        StartHeartbeat();
    }

    private void StartHeartbeat()
    {
        _heartbeatTimer = Dispatcher.CreateTimer();
        _heartbeatTimer.Interval = TimeSpan.FromSeconds(3); // Gửi mỗi 3 giây theo yêu cầu
        _heartbeatTimer.Tick += async (s, e) =>
        {
            try
            {
                // Lấy ID người dùng từ Preferences (Nếu chưa đăng nhập dùng Guest)
                string userId = Microsoft.Maui.Storage.Preferences.Get("UserId", "Guest_User");
                await _httpClient.PostAsync($"api/Stats/heartbeat/{userId}", null);
            }
            catch { /* Chặn lỗi để không treo App */ }
        };
        _heartbeatTimer.Start();
    }
}
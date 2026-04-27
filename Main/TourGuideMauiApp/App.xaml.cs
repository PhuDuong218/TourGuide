using Microsoft.AspNetCore.SignalR.Client;
using TourGuideMauiApp.Views;

namespace TourGuideMauiApp;

public partial class App : Application
{
    // 🔥 FIX CS8618: Thêm dấu "?" để báo cho trình biên dịch biết biến này có thể null lúc mới khởi chạy
    public static HubConnection? HubConnection { get; private set; }

    public App()
    {
        InitializeComponent();

        // Khởi tạo kết nối SignalR
        InitRealTime();
    }

    private async void InitRealTime()
    {
        // Thay link Server của bạn vào đây
        HubConnection = new HubConnectionBuilder()
            .WithUrl("https://gzm4vrwg-7054.asse.devtunnels.ms/activeUserHub")
            .WithAutomaticReconnect()
            .Build();

        HubConnection.On<string>("RoleUpgraded", async (upgradedUserId) =>
        {
            string? currentUserId = Preferences.Get("LoggedInUserId", null);

            if (currentUserId == upgradedUserId)
            {
                Preferences.Set("UserRole", "owner");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // 🔥 FIX CS0618: Thay thế MainPage bằng chuẩn Windows[0].Page của .NET 9
                    if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
                    {
                        await Application.Current.Windows[0].Page!.DisplayAlert(
                            "🎉 Chúc mừng!",
                            "Yêu cầu của bạn đã được duyệt. Bạn chính thức là Chủ quán đối tác!",
                            "Tuyệt vời");
                    }
                });
            }
        });

        try
        {
            await HubConnection.StartAsync();
            Console.WriteLine("SignalR: Đã kết nối thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Lỗi kết nối: {ex.Message}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        bool isUser = Preferences.ContainsKey("LoggedInUserId");
        bool isGuest = Preferences.ContainsKey("AnonymousDeviceId");

        Page startingPage;
        if (isUser || isGuest)
        {
            startingPage = new AppShell();
        }
        else
        {
            startingPage = new NavigationPage(new LoginPage());
        }

        return new Window(startingPage);
    }
}
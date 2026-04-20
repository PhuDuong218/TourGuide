using Microsoft.AspNetCore.SignalR.Client;
using TourGuideMauiApp.Views;

namespace TourGuideMauiApp;

public partial class App : Application
{
    // Tạo biến static để giữ kết nối xuyên suốt vòng đời App
    public static HubConnection HubConnection { get; private set; }

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
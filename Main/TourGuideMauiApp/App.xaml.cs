using TourGuideMauiApp.Views;

namespace TourGuideMauiApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    // ĐÃ SỬA: Dùng hàm CreateWindow của .NET 9 thay vì MainPage
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
            // ĐÃ SỬA: Bọc LoginPage vào NavigationPage để có thể chuyển sang RegisterPage
            startingPage = new NavigationPage(new LoginPage()); 
        }

        return new Window(startingPage);
    }
}
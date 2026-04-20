using TourGuideMauiApp.Helpers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TourGuideMauiApp.Views;

public partial class LoginPage : ContentPage
{
    // CỰC KỲ QUAN TRỌNG: Thay URL này bằng link Dev Tunnels của Server API của bạn
    private readonly string ApiBaseUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api";

    public LoginPage()
    {
        InitializeComponent();
    }

    // Lớp để hứng dữ liệu ID từ Server trả về
    public class LoginResponse
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = txtUsername.Text;
        string password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu", "OK");
            return;
        }

        try
        {
            using var client = new HttpClient();
            var loginData = new { Username = username, Password = password };

            // Gọi API kiểm tra trong Database
            var response = await client.PostAsJsonAsync($"{ApiBaseUrl}/Auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                // Nếu API trả về OK (đúng tài khoản)
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result != null && !string.IsNullOrEmpty(result.UserId))
                {
                    // Lưu đúng mã UserID từ Database (VD: U001, U002) vào máy
                    AuthHelper.Login(result.UserId);

                    await DisplayAlert("Thành công", "Đăng nhập thành công!", "OK");

                    // ĐÃ SỬA: Đổi màn hình bằng cách can thiệp vào Window hiện tại (Chuẩn .NET 9)
                    if (Application.Current?.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                    }
                }
            }
            else
            {
                // Nếu API trả về lỗi 401 Unauthorized
                await DisplayAlert("Thất bại", "Sai tên đăng nhập hoặc mật khẩu", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Lỗi mạng", "Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại mạng hoặc URL API.", "OK");
        }
    }

    private async void OnGuestClicked(object sender, EventArgs e)
    {
        // Xóa thông tin đăng nhập cũ (nếu có) để ép app dùng Device ID ẩn danh
        AuthHelper.Logout();

        // Gọi hàm để tự sinh mã khách (DEV-xxx) lưu vào máy
        AuthHelper.GetCurrentUserId();

        await DisplayAlert("Xin chào", "Bạn đang dùng app với tư cách khách.", "OK");

        // ĐÃ SỬA: Đổi màn hình bằng cách can thiệp vào Window hiện tại (Chuẩn .NET 9)
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new AppShell();
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        // Mở trang đăng ký dưới dạng một màn hình đè lên (Modal)
        await Navigation.PushModalAsync(new RegisterPage());
    }
}
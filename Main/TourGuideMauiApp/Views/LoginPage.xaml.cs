using TourGuideMauiApp.Helpers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TourGuideMauiApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly string ApiBaseUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api";

    public LoginPage()
    {
        InitializeComponent();
    }

    // 1. CẬP NHẬT: Thêm thuộc tính Role để hứng dữ liệu từ API
    public class LoginResponse
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
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

            var response = await client.PostAsJsonAsync($"{ApiBaseUrl}/Auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result != null && !string.IsNullOrEmpty(result.UserId))
                {
                    // 2. KIỂM TRA QUYỀN (ROLE)
                    // Chuyển về chữ thường để so sánh cho chính xác
                    string userRole = result.Role?.ToLower() ?? "";

                    if (userRole == "admin" || userRole == "owner")
                    {
                        // Hiển thị thông báo chặn Admin/Owner
                        await DisplayAlert("Từ chối truy cập",
                            "Tài khoản Admin/Owner chỉ dành cho Web quản trị. Vui lòng sử dụng tài khoản User trên di động!",
                            "OK");
                        return; // Thoát hàm, không cho vào AppShell
                    }

                    // Nếu không phải admin/owner (tức là user), thì cho phép vào
                    AuthHelper.Login(result.UserId);
                    await DisplayAlert("Thành công", "Đăng nhập thành công!", "OK");

                    if (Application.Current?.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                    }
                }
            }
            else
            {
                await DisplayAlert("Thất bại", "Sai tên đăng nhập hoặc mật khẩu", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi mạng", "Không thể kết nối đến máy chủ. Lỗi: " + ex.Message, "OK");
        }
    }

    private async void OnGuestClicked(object sender, EventArgs e)
    {
        AuthHelper.Logout();
        AuthHelper.GetCurrentUserId();

        await DisplayAlert("Xin chào", "Bạn đang dùng app với tư cách khách.", "OK");

        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new AppShell();
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new RegisterPage());
    }
}
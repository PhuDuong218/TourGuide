using System.Net.Http.Json;

namespace TourGuideMauiApp.Views;

public partial class RegisterPage : ContentPage
{
    private readonly string ApiBaseUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api";

    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterSubmitClicked(object sender, EventArgs e)
    {
        // 1. Kiểm tra bỏ trống
        if (string.IsNullOrWhiteSpace(txtRegUsername.Text) || string.IsNullOrWhiteSpace(txtRegEmail.Text))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ Tên đăng nhập và Email", "OK");
            return;
        }

        // 2. Kiểm tra mật khẩu khớp nhau (Tránh người dùng gõ nhầm)
        if (txtRegPassword.Text != txtRegConfirmPassword.Text)
        {
            await DisplayAlert("Lỗi", "Mật khẩu xác nhận không khớp!", "OK");
            return;
        }

        try
        {
            using var client = new HttpClient();
            var regData = new
            {
                FullName = txtRegFullName.Text,
                Email = txtRegEmail.Text,
                Username = txtRegUsername.Text,
                Password = txtRegPassword.Text
            };

            var response = await client.PostAsJsonAsync($"{ApiBaseUrl}/Auth/register", regData);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Thành công", "Tài khoản đã được tạo!", "OK");
                await Navigation.PopModalAsync();
            }
            else
            {
                // Đọc thông báo lỗi từ Server (Ví dụ: "Tên đăng nhập đã tồn tại")
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Thất bại", "Không thể đăng ký. Có thể tên đăng nhập đã tồn tại.", "OK");
            }
        }
        catch
        {
            await DisplayAlert("Lỗi", "Không thể kết nối máy chủ. Hãy kiểm tra URL API của bạn.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
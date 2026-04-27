using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;

namespace TourGuideMauiApp.Views;

public partial class RegisterOwnerPage : ContentPage
{
    private readonly DatabaseService _dbService = new();

    public RegisterOwnerPage() { InitializeComponent(); }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập Tên và Số điện thoại", "OK");
            return;
        }

        var request = new OwnerRequestDTO
        {
            FullName = txtName.Text,
            Phone = txtPhone.Text,
            Email = txtEmail.Text,
            PlaceName = txtPlace.Text,
            Address = txtAddress.Text
        };

        bool success = await _dbService.SendOwnerRequestAsync(request);
        if (success)
        {
            await DisplayAlert("Thành công", "Đã gửi yêu cầu đăng ký Chủ quán. Hệ thống sẽ sớm xét duyệt!", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Lỗi", "Không thể gửi yêu cầu lúc này. Vui lòng kiểm tra mạng.", "OK");
        }
    }
}
using System;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;

namespace TourGuideMauiApp.Views;

public partial class SettingsPage : ContentPage
{
    // Cài đặt sẵn các mã màu để đổi màu nút bấm
    private readonly Color _activeBg = Color.FromArgb("#6200EE");
    private readonly Color _inactiveBg = Color.FromArgb("#F5F5F5");
    private readonly Color _activeText = Colors.White;
    private readonly Color _inactiveText = Color.FromArgb("#555555");

    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        // 1. Load Ngôn ngữ
        string currentLang = Preferences.Get("AppLanguage", "vi");
        UpdateLanguageUI(currentLang);

        // 2. Load trạng thái công tắc Tự động phát
        switchAutoPlay.IsToggled = Preferences.Get("AutoPlayTTS", true);

        // 3. Load Tốc độ đọc
        double speed = Preferences.Get("TTSSpeed", 1.0);
        UpdateSpeedUI(speed);
    }

    // =========================================
    // HÀM XỬ LÝ TỐC ĐỘ ĐỌC VÀ TỰ ĐỘNG PHÁT (PHẦN BỊ THIẾU)
    // =========================================

    private void OnAutoPlayToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("AutoPlayTTS", e.Value);
    }

    // Đây chính là các hàm hệ thống đang báo thiếu (XC0002)
    private void OnSpeed05Tapped(object sender, EventArgs e) => SetSpeed(0.5);
    private void OnSpeed10Tapped(object sender, EventArgs e) => SetSpeed(1.0);
    private void OnSpeed15Tapped(object sender, EventArgs e) => SetSpeed(1.5);
    private void OnSpeed20Tapped(object sender, EventArgs e) => SetSpeed(2.0);

    private void SetSpeed(double speed)
    {
        Preferences.Set("TTSSpeed", speed);
        UpdateSpeedUI(speed); // Cập nhật màu nút ngay lập tức
    }

    private void UpdateSpeedUI(double speed)
    {
        // Reset màu tất cả các nút tốc độ
        btnS05.BackgroundColor = _inactiveBg; btnS05.TextColor = _inactiveText;
        btnS10.BackgroundColor = _inactiveBg; btnS10.TextColor = _inactiveText;
        btnS15.BackgroundColor = _inactiveBg; btnS15.TextColor = _inactiveText;
        btnS20.BackgroundColor = _inactiveBg; btnS20.TextColor = _inactiveText;

        // Bôi đậm nút đang được chọn
        if (speed == 0.5) { btnS05.BackgroundColor = _activeBg; btnS05.TextColor = _activeText; }
        else if (speed == 1.5) { btnS15.BackgroundColor = _activeBg; btnS15.TextColor = _activeText; }
        else if (speed == 2.0) { btnS20.BackgroundColor = _activeBg; btnS20.TextColor = _activeText; }
        else { btnS10.BackgroundColor = _activeBg; btnS10.TextColor = _activeText; }
    }

    // =========================================
    // HÀM XỬ LÝ NGÔN NGỮ (CỦA BẠN ĐÃ CÓ SẴN)
    // =========================================

    private void OnLangViTapped(object sender, EventArgs e) => SetLang("vi");
    private void OnLangEnTapped(object sender, EventArgs e) => SetLang("en");
    private void OnLangFrTapped(object sender, EventArgs e) => SetLang("fr");
    private void OnLangJaTapped(object sender, EventArgs e) => SetLang("ja");

    private void SetLang(string lang)
    {
        Preferences.Set("AppLanguage", lang);
        UpdateLanguageUI(lang);
    }

    private void UpdateLanguageUI(string lang)
    {
        // Reset màu tất cả các nút ngôn ngữ
        btnVI.BackgroundColor = _inactiveBg; btnVI.TextColor = _inactiveText;
        btnEN.BackgroundColor = _inactiveBg; btnEN.TextColor = _inactiveText;
        btnFR.BackgroundColor = _inactiveBg; btnFR.TextColor = _inactiveText;
        btnJA.BackgroundColor = _inactiveBg; btnJA.TextColor = _inactiveText;

        // Bôi đậm nút đang được chọn
        switch (lang.ToLower())
        {
            case "vi": btnVI.BackgroundColor = _activeBg; btnVI.TextColor = _activeText; break;
            case "en": btnEN.BackgroundColor = _activeBg; btnEN.TextColor = _activeText; break;
            case "fr": btnFR.BackgroundColor = _activeBg; btnFR.TextColor = _activeText; break;
            case "ja": btnJA.BackgroundColor = _activeBg; btnJA.TextColor = _activeText; break;
        }
    }

    // =========================================
    // HÀM CHUYỂN TRANG
    // =========================================
    private async void OnRegisterOwnerTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterOwnerPage());
    }

    // =========================================
    // HÀM ĐĂNG XUẤT
    // =========================================
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Xác nhận", "Bạn có chắc chắn muốn đăng xuất?", "Có", "Không");
        if (confirm)
        {
            // Xóa bộ nhớ
            TourGuideMauiApp.Helpers.AuthHelper.Logout();

            // Tráo màn hình về lại trang Đăng nhập (Chuẩn .NET 9)
            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new LoginPage();
            }
        }
    }
}
using Microsoft.Maui.Storage;
using System;

namespace TourGuideMauiApp.Helpers;

public static class AuthHelper
{
    // Hàm này tự động quyết định xem nên dùng ID thật hay ID ẩn danh
    public static string GetCurrentUserId()
    {
        // 1. Kiểm tra xem người dùng có đang đăng nhập không
        string loggedInUserId = Preferences.Get("LoggedInUserId", string.Empty);

        if (!string.IsNullOrEmpty(loggedInUserId))
        {
            // Nếu đã đăng nhập, trả về mã thật (VD: U003, U004)
            return loggedInUserId;
        }

        // 2. Nếu KHÔNG đăng nhập (Khách), sử dụng Device ID ẩn danh
        string deviceId = Preferences.Get("AnonymousDeviceId", string.Empty);
        if (string.IsNullOrEmpty(deviceId))
        {
            // Tự động tạo mã ngẫu nhiên (VD: DEV-A1B2C3D4E5) và lưu lại vào máy
            deviceId = "DEV-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            Preferences.Set("AnonymousDeviceId", deviceId);
        }

        return deviceId;
    }

    // Hàm gọi khi người dùng Đăng nhập thành công
    public static void Login(string userId)
    {
        Preferences.Set("LoggedInUserId", userId);
    }

    // Hàm gọi khi người dùng Đăng xuất
    public static void Logout()
    {
        Preferences.Remove("LoggedInUserId");
    }
}
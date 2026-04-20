using System.Globalization;

namespace TourGuideMauiApp.Converters;

public class ImageUrlConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? img = value as string;

        // Nếu Database không có ảnh, trả về ảnh mặc định
        if (string.IsNullOrEmpty(img))
            return "https://via.placeholder.com/400x200?text=No+Image";

        // Gắn link Dev Tunnel của bạn vào trước tên ảnh
        // (Nếu ngày mai link Tunnel đổi, bạn chỉ cần vào ĐÚNG FILE NÀY để sửa link)
        string baseUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms";
        return $"{baseUrl}/uploads/{img}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
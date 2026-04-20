namespace WebCMS.Models
{
    public class DashboardViewModel
    {
        // Các thông số tổng quan (Card)
        public int TotalUsers { get; set; }
        public int TotalQR { get; set; }
        public int TotalQRScans { get; set; }      // Tổng lượt quét QR
        public int TotalAppUsage { get; set; }    // Lượt sử dụng App (trước đây bạn để là TotalTTS)
        public int ActiveToday { get; set; }
        public int TotalLanguages { get; set; } = 4; // Mặc định 4 ngôn ngữ: VI, EN, FR, JA

        // Dữ liệu cho biểu đồ Bar (Top 5)
        public List<string> ChartLabels { get; set; } = new();
        public List<int> ChartValues { get; set; } = new();

        // Dữ liệu cho biểu đồ Line (Xu hướng 7 ngày)
        public List<string> TrendLabels { get; set; } = new();
        public List<int> InstallTrend { get; set; } = new();
        public List<int> ActiveTrend { get; set; } = new();
        public List<int> ScanTrend { get; set; } = new();
    }
}
namespace WebCMS.Models
{
    public class DashboardViewModel
    {
        // 4 Thẻ thống kê tổng quan
        public int TotalLanguages { get; set; }
        public int TotalQR { get; set; }
        public int TotalQRScans { get; set; }
        public int TotalAppUsage { get; set; }
        public int ActiveToday { get; set; }

        // Dữ liệu cho biểu đồ Line (Xu hướng 7 ngày)
        public List<string> TrendLabels { get; set; } = new List<string>();
        public List<int> InstallTrend { get; set; } = new List<int>();
        public List<int> ActiveTrend { get; set; } = new List<int>();
        public List<int> ScanTrend { get; set; } = new List<int>();

        // Dữ liệu cho biểu đồ Bar (Top 5 POI)
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartValues { get; set; } = new List<int>();
    }
}
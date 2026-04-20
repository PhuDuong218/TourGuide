using WebCMS.Models;

namespace WebCMS.Services
{
    public class StatsService
    {
        private readonly HttpClient _http;
        public StatsService(HttpClient http) => _http = http;

        public async Task<DashboardViewModel> GetSummaryAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<DashboardViewModel>("Stats/summary") ?? new DashboardViewModel();
            }
            catch { return new DashboardViewModel(); }
        }

        public async Task<DashboardViewModel> GetFullDashboardAsync()
        {
            var summary = await GetSummaryAsync();
            try
            {
                var chart = await _http.GetFromJsonAsync<ChartResponse>("Stats/chart-data");
                if (chart != null)
                {
                    summary.ChartLabels = chart.Top5Labels; // Lấy đúng tên biến
                    summary.ChartValues = chart.Top5Data;
                    summary.TrendLabels = chart.TrendLabels;
                    summary.InstallTrend = chart.InstallData;
                    summary.ActiveTrend = chart.ActiveData;
                    summary.ScanTrend = chart.ScanData;
                }
            }
            catch { }
            return summary;
        }

        // PHẢI KHỚP TÊN VỚI JSON TỪ SERVER TRẢ VỀ
        private class ChartResponse
        {
            public List<string> Top5Labels { get; set; } = new();
            public List<int> Top5Data { get; set; } = new();
            public List<string> TrendLabels { get; set; } = new();
            public List<int> InstallData { get; set; } = new();
            public List<int> ActiveData { get; set; } = new();
            public List<int> ScanData { get; set; } = new();
        }
    }
}
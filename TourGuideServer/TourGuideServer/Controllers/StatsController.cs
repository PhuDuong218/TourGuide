using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var today = DateTime.Today;

            // 1. Thống kê tổng quan
            var totalLanguages = await _context.Languages.CountAsync();
            var totalQR = await _context.QRCodes.CountAsync();
            var totalQRScans = await _context.VisitHistories.CountAsync(v => v.ScanMethod == "QR_Scan");
            var totalAppUsage = await _context.VisitHistories.CountAsync();
            var activeToday = await _context.VisitHistories
                .Where(v => v.VisitTime >= today)
                .Select(v => v.UserID)
                .Distinct()
                .CountAsync();

            // 2. Thống kê biểu đồ xu hướng 7 ngày
            var trendLabels = new List<string>();
            var installTrend = new List<int>();
            var activeTrend = new List<int>();
            var scanTrend = new List<int>();

            var startDate = today.AddDays(-6);
            for (int i = 0; i <= 6; i++)
            {
                var currentDate = startDate.AddDays(i);
                var nextDate = currentDate.AddDays(1);

                trendLabels.Add(currentDate.ToString("dd/MM"));

                installTrend.Add(await _context.Users.CountAsync(u => u.Role == "user" && u.CreatedAt >= currentDate && u.CreatedAt < nextDate));
                activeTrend.Add(await _context.VisitHistories.CountAsync(v => v.VisitTime >= currentDate && v.VisitTime < nextDate));
                scanTrend.Add(await _context.VisitHistories.CountAsync(v => v.ScanMethod == "QR_Scan" && v.VisitTime >= currentDate && v.VisitTime < nextDate));
            }

            // 3. Thống kê Top 5 POI
            var topPois = await _context.POIs.OrderByDescending(p => p.ListenCount).Take(5).ToListAsync();
            var chartLabels = topPois.Select(p => p.RestaurantName ?? "Chưa đặt tên").ToList();
            var chartValues = topPois.Select(p => p.ListenCount).ToList();

            // Trả về JSON 
            return Ok(new
            {
                totalLanguages,
                totalQR,
                totalQRScans,
                totalAppUsage,
                activeToday,
                trendLabels,
                installTrend,
                activeTrend,
                scanTrend,
                chartLabels,
                chartValues
            });
        }

        [HttpGet("active-users")]
        public async Task<IActionResult> GetRealTimeActive()
        {
            var fiveMinsAgo = DateTime.Now.AddMinutes(-5);
            var activeCount = await _context.VisitHistories
                .Where(v => v.VisitTime >= fiveMinsAgo)
                .Select(v => v.UserID)
                .Distinct()
                .CountAsync();

            return Ok(new { count = activeCount });
        }
    }
}
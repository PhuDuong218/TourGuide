using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

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

            // 1. Thống kê tổng quan (Giữ nguyên như cũ của bạn)
            var totalLanguages = await _context.Languages.CountAsync();
            var totalPOI = await _context.POIs.CountAsync();
            var totalQR = await _context.QRCodes.CountAsync();
            var totalListens = await _context.POIs.SumAsync(p => p.ListenCount);

            var activeToday = await _context.VisitHistories
                .Where(v => v.VisitTime >= today)
                .Select(v => v.UserID)
                .Distinct()
                .CountAsync();

            // ==========================================
            // 2. Thống kê Top 5 POI mặc định lúc load trang (ĐÃ ĐỔI THÀNH TẤT CẢ THỜI GIAN)
            // ==========================================
            var topPoisAllTime = await _context.POIs
                .OrderByDescending(p => p.ListenCount)
                .Take(5)
                .ToListAsync();

            var chartLabels = new List<string>();
            var chartValues = new List<int>();

            foreach (var poi in topPoisAllTime)
            {
                chartLabels.Add(poi.RestaurantName ?? "Chưa đặt tên");
                chartValues.Add(poi.ListenCount);
            }

            // Trả về JSON 
            return Ok(new
            {
                totalLanguages,
                totalPOI,
                totalQR,
                totalListens,
                activeToday,
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
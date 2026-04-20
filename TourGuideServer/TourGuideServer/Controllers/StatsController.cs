using Microsoft.AspNetCore.Mvc;
using TourGuideServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[Route("api/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const string ActiveUsersKey = "ActiveUsers_Heartbeat";

    public StatsController(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // API Heartbeat: App gọi mỗi 3 giây để báo danh
    [HttpPost("heartbeat/{userId}")]
    public IActionResult Heartbeat(string userId)
    {
        var activeUsers = _cache.Get<Dictionary<string, DateTime>>(ActiveUsersKey) ?? new();
        activeUsers[userId] = DateTime.Now;
        // Lưu cache trong 1 phút để dự phòng
        _cache.Set(ActiveUsersKey, activeUsers, TimeSpan.FromMinutes(1));
        return Ok();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        // Tính toán số người thực sự online trong 10 giây qua (Heartbeat)
        var activeUsers = _cache.Get<Dictionary<string, DateTime>>(ActiveUsersKey) ?? new();
        int realTimeActive = activeUsers.Count(u => (DateTime.Now - u.Value).TotalSeconds <= 10);

        var stats = new
        {
            TotalUsers = await _context.Users.CountAsync(u => u.Role == "user"),
            TotalQR = await _context.POIs.CountAsync(),
            TotalQRScans = await _context.VisitHistories.CountAsync(), // Tổng lượt quét QR
            TotalAppUsage = await _context.POIs.SumAsync(p => p.ListenCount), // Tổng lượt nghe
            ActiveToday = realTimeActive
        };
        return Ok(stats);
    }

    [HttpGet("chart-data")]
    public async Task<IActionResult> GetChartData()
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.Today.AddDays(-i))
            .Reverse()
            .ToList();

        var trendLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
        var installData = new List<int>();
        var activeData = new List<int>();
        var scanData = new List<int>();

        foreach (var date in last7Days)
        {
            // Xử lý kiểu DateTime? bằng .HasValue
            installData.Add(await _context.Users.CountAsync(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Date == date.Date));
            activeData.Add(await _context.VisitHistories.Where(v => v.VisitTime.Date == date.Date).Select(v => v.UserID).Distinct().CountAsync());
            scanData.Add(await _context.VisitHistories.CountAsync(v => v.VisitTime.Date == date.Date));
        }

        var top5Pois = await _context.POIs
            .OrderByDescending(p => p.ListenCount)
            .Take(5)
            .Select(p => new { p.RestaurantName, p.ListenCount }) // Dùng RestaurantName khớp Model
            .ToListAsync();

        return Ok(new
        {
            TrendLabels = trendLabels,
            InstallData = installData,
            ActiveData = activeData,
            ScanData = scanData,
            Top5Labels = top5Pois.Select(p => p.RestaurantName).ToList(),
            Top5Data = top5Pois.Select(p => p.ListenCount).ToList()
        });
    }
}
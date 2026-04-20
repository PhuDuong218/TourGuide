using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisitHistoryController(AppDbContext context)
        {
            _context = context;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VisitHistories
                .Include(v => v.POI)
                .Include(v => v.User)
                .OrderByDescending(v => v.VisitTime)
                .Select(v => new
                {
                    v.VisitID,
                    v.VisitTime,
                    v.ScanMethod,
                    v.LanguageUsed,
                    // Thêm dòng này để Client có ID mà lọc
                    OwnerID = v.POI != null ? v.POI.OwnerID : null,
                    POIName = v.POI != null ? v.POI.RestaurantName : "N/A",
                    Username = v.User != null ? v.User.FullName : "Khách"
                })
                .ToListAsync();

            return Ok(data);
        }

        // ================= POST: NHẬN LỊCH SỬ TỪ APP =================
        [HttpPost]
        public async Task<IActionResult> LogVisit([FromBody] VisitHistory request)
        {
            try
            {
                // 1. Bổ sung thời gian hiện tại lúc quét mã
                request.VisitTime = DateTime.Now;

                // 2. Mặc định phương thức quét nếu trống
                if (string.IsNullOrEmpty(request.ScanMethod))
                {
                    request.ScanMethod = "QR_Scan";
                }

                // 3. Lưu lịch sử vào Database
                _context.VisitHistories.Add(request);

                // 4. Cộng thêm 1 lượt nghe vào POI (Để biểu đồ Top POI tăng lên)
                var poi = await _context.POIs.FindAsync(request.POIID);
                if (poi != null)
                {
                    poi.ListenCount = (poi.ListenCount) + 1; // Tăng lượt nghe
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã lưu lịch sử quét QR thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ================= DASHBOARD STATS =================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _context.VisitHistories
                .Include(v => v.POI)
                .GroupBy(v => new
                {
                    v.POIID,
                    POIName = v.POI != null ? v.POI.RestaurantName : "N/A"
                })
                .Select(g => new
                {
                    POIID = g.Key.POIID,
                    POIName = g.Key.POIName,
                    Total = g.Count(),

                    // CHUẨN HÓA ScanMethod khớp với App MAUI
                    ByGPS = g.Count(v => v.ScanMethod == "GPS"),
                    ByQR = g.Count(v => v.ScanMethod == "QR_Scan"),

                    LastVisit = g.Max(v => v.VisitTime)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return Ok(stats);
        }
    }
}
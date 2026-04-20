using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

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
                // 🔥 Thêm dòng này để Client có ID mà lọc
                OwnerID = v.POI != null ? v.POI.OwnerID : null,
                POIName = v.POI != null ? v.POI.RestaurantName : "N/A",
                Username = v.User != null ? v.User.FullName : "Khách"
            })
            .ToListAsync();

            return Ok(data);
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

                    // 🔥 CHUẨN HÓA ScanMethod
                    ByGPS = g.Count(v => v.ScanMethod == "GPS"),
                    ByQR = g.Count(v => v.ScanMethod == "QR"),

                    LastVisit = g.Max(v => v.VisitTime)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return Ok(stats);
        }
    }
}
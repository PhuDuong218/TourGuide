using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    // ══════════════════════════════════════════════════════════════
    // USER CONTROLLER
    // ══════════════════════════════════════════════════════════════
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context) => _context = context;

        // GET /api/User
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .OrderBy(u => u.UserID)
                .Select(u => new {
                    u.UserID,
                    u.Username,
                    u.Email,
                    u.FullName,
                    u.Role
                    // Không trả về PasswordHash
                })
                .ToListAsync();
            return Ok(users);
        }

        // GET /api/User/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new { user.UserID, user.Username, user.Email, user.FullName, user.Role });
        }

        // POST /api/User — tạo user mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            var exists = await _context.Users.AnyAsync(u => u.Username == req.Username);
            if (exists)
                return Conflict(new { message = $"Username '{req.Username}' đã tồn tại." });

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = req.Password,   // MVP: plain text
                FullName = req.FullName,
                Role = req.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.UserID, user.Username, user.Email, user.FullName, user.Role });
        }

        // PUT /api/User/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Email = req.Email;
            user.FullName = req.FullName;
            user.Role = req.Role;
            if (!string.IsNullOrEmpty(req.Password))
                user.PasswordHash = req.Password;

            await _context.SaveChangesAsync();
            return Ok(new { user.UserID, user.Username, user.Email, user.FullName, user.Role });
        }

        // DELETE /api/User/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public string? Password { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // VISIT HISTORY CONTROLLER
    // ══════════════════════════════════════════════════════════════
    [ApiController]
    [Route("api/[controller]")]
    public class VisitHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public VisitHistoryController(AppDbContext context) => _context = context;

        // GET /api/VisitHistory?limit=100
        [HttpGet]
        public async Task<IActionResult> GetAll(int limit = 100)
        {
            var visits = await _context.VisitHistories
                .Include(v => v.POI)
                .Include(v => v.User)
                .OrderByDescending(v => v.VisitTime)
                .Take(limit)
                .Select(v => new {
                    v.VisitID,
                    v.VisitTime,
                    POIName = v.POI != null ? v.POI.RestaurantName : "–",
                    v.POIID,
                    Username = v.User != null ? v.User.Username : "Ẩn danh",
                    v.ScanMethod,
                    v.LanguageUsed,
                    v.UserLat,
                    v.UserLon
                })
                .ToListAsync();
            return Ok(visits);
        }

        // GET /api/VisitHistory/poi/{poiId}
        [HttpGet("poi/{poiId:int}")]
        public async Task<IActionResult> GetByPOI(int poiId)
        {
            var visits = await _context.VisitHistories
                .Where(v => v.POIID == poiId)
                .OrderByDescending(v => v.VisitTime)
                .ToListAsync();
            return Ok(visits);
        }

        // GET /api/VisitHistory/stats — thống kê lượt xem theo POI
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _context.VisitHistories
                .Include(v => v.POI)
                .GroupBy(v => new { v.POIID, POIName = v.POI != null ? v.POI.RestaurantName : "–" })
                .Select(g => new {
                    g.Key.POIID,
                    g.Key.POIName,
                    Total = g.Count(),
                    ByGPS = g.Count(v => v.ScanMethod == "GPS_Trigger"),
                    ByQR = g.Count(v => v.ScanMethod == "QR_Scan"),
                    LastVisit = g.Max(v => v.VisitTime)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();
            return Ok(stats);
        }

        // DELETE /api/VisitHistory/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var visit = await _context.VisitHistories.FindAsync(id);
            if (visit == null) return NotFound();
            _context.VisitHistories.Remove(visit);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Hubs;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerRequestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ActiveUserHub> _hubContext; // 🔥 SignalR

        public OwnerRequestController(AppDbContext context, IHubContext<ActiveUserHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OwnerRequest request)
        {
            try
            {
                var count = await _context.OwnerRequests.CountAsync();
                request.RequestID = "REQ" + (count + 1).ToString("D3");
                request.Status = "Pending";
                request.CreatedAt = DateTime.Now;

                _context.OwnerRequests.Add(request);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Gửi thành công!", id = request.RequestID });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OwnerRequests.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(data);
        }

        // 🔥 SỬA CHỖ NÀY: Hàm Approve đã được cập nhật chuẩn theo Model User
        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> Approve(string id)
        {
            var req = await _context.OwnerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Approved";

            // 1. Tìm User theo Email (Vì Model User chỉ có Email)
            // Có thêm điều kiện kiểm tra req.Email khác rỗng để tránh lỗi tìm nhầm
            User? user = null;
            if (!string.IsNullOrEmpty(req.Email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            }

            // 2. Nếu tìm thấy tài khoản, tiến hành đổi Role
            if (user != null)
            {
                user.Role = "owner";
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            // 3. Bắn thông báo xuống App qua SignalR
            if (user != null)
            {
                await _hubContext.Clients.All.SendAsync("RoleUpgraded", user.UserID);
            }

            return Ok(new { message = "Đã duyệt và nâng cấp quyền Chủ quán!" });
        }

        [HttpPost("Reject/{id}")]
        public async Task<IActionResult> Reject(string id)
        {
            var req = await _context.OwnerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối!" });
        }
    }
}
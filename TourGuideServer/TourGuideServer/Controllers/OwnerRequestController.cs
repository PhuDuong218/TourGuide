using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerRequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OwnerRequestController(AppDbContext context)
        {
            _context = context;
        }

        // 🟢 1. CỔNG NHẬN (App MAUI gọi hàm này)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OwnerRequest request)
        {
            try
            {
                // 1. TỰ TẠO ID (Vì nvarchar(10) không tự tăng)
                var count = await _context.OwnerRequests.CountAsync();
                // Tạo mã kiểu REQ001, REQ002...
                request.RequestID = "REQ" + (count + 1).ToString("D3");

                // 2. Gán các giá trị mặc định
                request.Status = "Pending";
                request.CreatedAt = DateTime.Now;

                _context.OwnerRequests.Add(request);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Gửi thành công!", id = request.RequestID });
            }
            catch (Exception ex)
            {
                // Trả về lỗi chi tiết từ SQL để dễ debug
                var innerError = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = innerError });
            }
        }

        // 🔵 2. LẤY DANH SÁCH (Web Admin gọi hàm này)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OwnerRequests
                                     .OrderByDescending(r => r.CreatedAt)
                                     .ToListAsync();
            return Ok(data);
        }

        // 🟡 3. DUYỆT YÊU CẦU
        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.OwnerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Approved";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã duyệt!" });
        }

        // 🔴 4. TỪ CHỐI YÊU CẦU
        [HttpPost("Reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.OwnerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối!" });
        }
    }
}
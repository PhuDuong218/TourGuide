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

        // Lấy danh sách yêu cầu (Mới nhất xếp lên đầu)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OwnerRequests
                                     .OrderByDescending(r => r.CreatedAt)
                                     .ToListAsync();
            return Ok(data);
        }

        // Đổi trạng thái thành Đã duyệt
        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.OwnerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Approved";
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Đổi trạng thái thành Từ chối
        [HttpPost("Reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.OwnerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QRCodeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? ownerId)
        {
            var query = _context.QRCodes
                .Include(q => q.POI)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                query = query.Where(q => q.POI != null && q.POI.OwnerID == ownerId);
            }

            var qrs = await query
                .OrderBy(q => q.POIID)
                .Select(q => new {
                    q.QRID,
                    q.POIID,
                    POIName = q.POI != null ? q.POI.RestaurantName : "–",
                    q.QRValue,
                    q.CreatedAt
                })
                .ToListAsync();
            return Ok(qrs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var qr = await _context.QRCodes
                .Include(q => q.POI)
                .FirstOrDefaultAsync(q => q.QRID == id);
            if (qr == null) return NotFound();
            return Ok(qr);
        }

        [HttpGet("poi/{poiId}")]
        public async Task<IActionResult> GetByPOI(string poiId)
        {
            var qrs = await _context.QRCodes
                .Where(q => q.POIID == poiId)
                .ToListAsync();
            return Ok(qrs);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQRRequest req)
        {
            if (string.IsNullOrEmpty(req.POIID) || string.IsNullOrEmpty(req.QRValue))
            {
                return BadRequest(new { message = "Thiếu dữ liệu POIID hoặc QRValue" });
            }

            var poi = await _context.POIs.FindAsync(req.POIID);
            if (poi == null)
                return NotFound(new { message = $"Không tìm thấy địa điểm có POIID = {req.POIID}" });

            var exists = await _context.QRCodes.AnyAsync(q => q.QRValue == req.QRValue);
            if (exists)
                return Conflict(new { message = $"Mã QR '{req.QRValue}' đã tồn tại." });

            string newQrId = !string.IsNullOrEmpty(req.QRID) ? req.QRID : "Q" + new Random().Next(100, 999).ToString();

            var qr = new QRCode
            {
                QRID = newQrId,
                POIID = req.POIID,
                QRValue = req.QRValue,
                CreatedAt = DateTime.Now
            };

            _context.QRCodes.Add(qr);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo mã QR thành công!", data = qr });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var qr = await _context.QRCodes.FindAsync(id);
            if (qr == null) return NotFound();

            _context.QRCodes.Remove(qr);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    //  Chỉ giữ lại 1 bản duy nhất viết HOA khớp với Database
    public class CreateQRRequest
    {
        public string? QRID { get; set; }
        public string POIID { get; set; } = string.Empty;
        public string QRValue { get; set; } = string.Empty;
    }
}
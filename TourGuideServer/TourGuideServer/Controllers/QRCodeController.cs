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

        // GET /api/QRCode — lấy tất cả QR kèm thông tin POI
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var qrs = await _context.QRCodes
                .Include(q => q.POI)
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

        // GET /api/QRCode/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var qr = await _context.QRCodes
                .Include(q => q.POI)
                .FirstOrDefaultAsync(q => q.QRID == id);
            if (qr == null) return NotFound();
            return Ok(qr);
        }

        // GET /api/QRCode/poi/{poiId} — lấy QR theo POI
        [HttpGet("poi/{poiId:int}")]
        public async Task<IActionResult> GetByPOI(int poiId)
        {
            var qrs = await _context.QRCodes
                .Where(q => q.POIID == poiId)
                .ToListAsync();
            return Ok(qrs);
        }

        // POST /api/QRCode — tạo QR mới
        // Body: { "poiId": 1, "qrValue": "QR_PHOHOA_001" }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQRRequest req)
        {
            // Kiểm tra POI tồn tại
            var poi = await _context.POIs.FindAsync(req.PoiId);
            if (poi == null)
                return NotFound(new { message = $"Không tìm thấy POI = {req.PoiId}" });

            // Kiểm tra QRValue trùng
            var exists = await _context.QRCodes.AnyAsync(q => q.QRValue == req.QrValue);
            if (exists)
                return Conflict(new { message = $"Mã QR '{req.QrValue}' đã tồn tại." });

            var qr = new QRCode
            {
                POIID = req.PoiId,
                QRValue = req.QrValue,
                CreatedAt = DateTime.Now
            };

            _context.QRCodes.Add(qr);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = qr.QRID }, qr);
        }

        // DELETE /api/QRCode/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var qr = await _context.QRCodes.FindAsync(id);
            if (qr == null) return NotFound();

            _context.QRCodes.Remove(qr);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateQRRequest
    {
        public int PoiId { get; set; }
        public string QrValue { get; set; } = string.Empty;
    }
}
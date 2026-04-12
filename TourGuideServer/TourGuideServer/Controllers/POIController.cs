using Microsoft.AspNetCore.Mvc;
using TourGuideServer.Data;
using TourGuideServer.Models;
using TourGuideServer.Services;
using Microsoft.EntityFrameworkCore;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly POIService _service;
        private readonly AppDbContext _context;

        public POIController(POIService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        // GET /api/POI?lang=vi
        [HttpGet]
        public async Task<IActionResult> GetAll(string lang = "vi")
        {
            var data = await _service.GetPOIsAsync(lang);
            return Ok(data);
        }

        // GET /api/POI/raw — lấy toàn bộ POI raw (cho web admin)
        [HttpGet("raw")]
        public async Task<IActionResult> GetAllRaw()
        {
            var pois = await _context.POIs
                .Include(p => p.Translations)
                .OrderByDescending(p => p.POIID)
                .ToListAsync();
            return Ok(pois);
        }

        // GET /api/POI/{id}?lang=vi
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, string lang = "vi")
        {
            var data = await _service.GetPOIByIdAsync(id, lang);
            if (data == null)
                return NotFound(new { message = $"Không tìm thấy POI với ID = {id}" });
            return Ok(data);
        }

        // GET /api/POI/{id}/raw — lấy raw POI (cho web admin edit)
        [HttpGet("{id:int}/raw")]
        public async Task<IActionResult> GetByIdRaw(int id)
        {
            var poi = await _context.POIs
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.POIID == id);
            if (poi == null) return NotFound();
            return Ok(poi);
        }

        // GET /api/POI/qr/{qrValue}?lang=vi
        [HttpGet("qr/{qrValue}")]
        public async Task<IActionResult> GetByQR(string qrValue, string lang = "vi")
        {
            var data = await _service.GetPOIByQRAsync(qrValue, lang);
            if (data == null)
                return NotFound(new { message = $"Không tìm thấy địa điểm với mã QR: {qrValue}" });
            return Ok(data);
        }

        // POST /api/POI — tạo POI mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] POI poi)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreatePOIAsync(poi);
            return CreatedAtAction(nameof(GetById), new { id = created.POIID }, created);
        }

        // PUT /api/POI/{id} — cập nhật POI
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] POI poi)
        {
            var existing = await _context.POIs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Không tìm thấy POI = {id}" });

            existing.RestaurantName = poi.RestaurantName;
            existing.Latitude       = poi.Latitude;
            existing.Longitude      = poi.Longitude;
            existing.Address        = poi.Address;
            existing.Category       = poi.Category;
            existing.ImageUrl       = poi.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE /api/POI/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeletePOIAsync(id);
            if (!ok) return NotFound(new { message = $"Không tìm thấy POI = {id}" });
            return NoContent();
        }
    }
}
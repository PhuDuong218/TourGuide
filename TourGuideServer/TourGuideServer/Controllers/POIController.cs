using Microsoft.AspNetCore.Mvc;
using TourGuideServer.Services;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly POIService _service;

        public POIController(POIService service)
        {
            _service = service;
        }

        // GET /api/POI?lang=vi
        // Lấy tất cả POI theo ngôn ngữ
        [HttpGet]
        public async Task<IActionResult> GetAll(string lang = "vi")
        {
            var data = await _service.GetPOIsAsync(lang);
            return Ok(data);
        }

        // GET /api/POI/5?lang=vi
        // Lấy 1 POI theo ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, string lang = "vi")
        {
            var data = await _service.GetPOIByIdAsync(id, lang);
            if (data == null)
                return NotFound(new { message = $"Không tìm thấy POI với ID = {id}" });

            return Ok(data);
        }

        // GET /api/POI/qr/QR_PHOHOA_001?lang=vi
        // Lấy POI theo mã QR (dùng cho tính năng quét mã)
        [HttpGet("qr/{qrValue}")]
        public async Task<IActionResult> GetByQR(string qrValue, string lang = "vi")
        {
            var data = await _service.GetPOIByQRAsync(qrValue, lang);
            if (data == null)
                return NotFound(new { message = $"Không tìm thấy địa điểm với mã QR: {qrValue}" });

            return Ok(data);
        }
    }
}
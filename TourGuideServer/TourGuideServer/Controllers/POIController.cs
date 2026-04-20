using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;
using TourGuideServer.Services;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly POIService _service;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Thêm biến môi trường

        public POIController(POIService service, AppDbContext context, IWebHostEnvironment env)
        {
            _service = service;
            _context = context;
            _env = env; // Inject môi trường
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? ownerId = null, string lang = "vi")
        {
            // Sử dụng Service để nó tự động Join bảng Dịch Thuật và Map sang DTO
            var data = await _service.GetPOIsAsync(lang, ownerId);
            return Ok(data);
        }

        [HttpGet("raw")]
        public async Task<IActionResult> GetAllRaw()
        {
            var pois = await _context.POIs
                .Include(p => p.Translations)
                .OrderByDescending(p => p.POIID)
                .ToListAsync();
            return Ok(pois);
        }

        [HttpGet("{id}")] // Bỏ :int
        public async Task<IActionResult> GetById(string id, string lang = "vi")
        {
            var data = await _service.GetPOIByIdAsync(id, lang);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(string id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null || string.IsNullOrEmpty(poi.Img))
            {
                return Redirect("https://via.placeholder.com/300");
            }

            // Đọc file từ thư mục uploads
            string filePath = Path.Combine(_env.WebRootPath, "uploads", poi.Img);
            if (!System.IO.File.Exists(filePath))
            {
                return Redirect("https://via.placeholder.com/300");
            }

            return File(System.IO.File.OpenRead(filePath), "image/jpeg");
        }

        [HttpGet("{id}/raw")]
        public async Task<IActionResult> GetByIdRaw(string id)
        {
            var poi = await _context.POIs
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.POIID == id);
            if (poi == null) return NotFound();
            return Ok(poi);
        }

        [HttpGet("qr/{qrValue}")]
        public async Task<IActionResult> GetByQR(string qrValue, string lang = "vi")
        {
            var data = await _service.GetPOIByQRAsync(qrValue, lang);
            if (data == null)
                return NotFound(new { message = $"Không tìm thấy địa điểm với mã QR: {qrValue}" });
            return Ok(data);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] POI poi, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                poi.Img = fileName;
            }

            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();
            return Ok(poi);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(string id, [FromForm] POI updatedPoi, IFormFile? imageFile)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            // 1. Chỉ cập nhật các trường cơ bản (Xóa phần Name và Description gây lỗi)
            poi.Latitude = updatedPoi.Latitude;
            poi.Longitude = updatedPoi.Longitude;
            poi.Category = updatedPoi.Category;

            // 2. Xử lý lưu file ảnh (nếu có chọn ảnh mới)
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                poi.Img = fileName; // Cập nhật tên file mới
            }
            // 3. Giữ lại ảnh cũ nếu form gửi lên tên ảnh cũ
            else if (!string.IsNullOrEmpty(updatedPoi.Img))
            {
                poi.Img = updatedPoi.Img;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", image = poi.Img });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var ok = await _service.DeletePOIAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // API: Tăng lượt xem khi mở Bottom Sheet
        [HttpPost("{id}/increment-view")]
        public async Task<IActionResult> IncrementView(string id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            poi.ViewCount += 1;
            await _context.SaveChangesAsync();
            return Ok(new { ViewCount = poi.ViewCount });
        }

        // API: Tăng lượt nghe khi ấn nút Nghe
        [HttpPost("{id}/increment-listen")]
        public async Task<IActionResult> IncrementListen(string id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            poi.ListenCount += 1;
            await _context.SaveChangesAsync();
            return Ok(new { ListenCount = poi.ListenCount });
        }

    }
}
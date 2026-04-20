using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TranslationController(AppDbContext context)
        {
            _context = context;
        }

        // ĐÃ THÊM: Hàm lấy tất cả bản dịch để WebCMS tìm ID lớn nhất
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.POITranslations.ToListAsync();
            return Ok(data);
        }

        [HttpGet("{poiId}")]
        public async Task<IActionResult> GetByPOI(string poiId)
        {
            var translations = await _context.POITranslations
                .Where(t => t.POIID == poiId)
                .OrderBy(t => t.LanguageCode)
                .ToListAsync();
            return Ok(translations);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] POITranslation translation)
        {
            if (translation == null) return BadRequest();

            var exists = await _context.POITranslations
                .AnyAsync(t => t.POIID == translation.POIID &&
                               t.LanguageCode == translation.LanguageCode);
            if (exists)
                return Conflict(new { message = $"Bản dịch '{translation.LanguageCode}' đã tồn tại." });

            // ĐÃ SỬA: Chỉ tự tạo mã ngẫu nhiên nếu WebCMS không gửi mã sang. 
            // Còn nếu WebCMS đã cấp mã T041, T042... thì giữ nguyên!
            if (string.IsNullOrEmpty(translation.TranslationID))
            {
                translation.TranslationID = Guid.NewGuid().ToString().Substring(0, 10);
            }

            _context.POITranslations.Add(translation);
            await _context.SaveChangesAsync();
            return Ok(translation);
        }

        [HttpPut("{poiId}/{lang}")]
        public async Task<IActionResult> Update(string poiId, string lang, [FromBody] POITranslation dto)
        {
            var existing = await _context.POITranslations
                .FirstOrDefaultAsync(t => t.POIID == poiId && t.LanguageCode == lang);

            if (existing == null) return NotFound();

            existing.DisplayName = dto.DisplayName;
            existing.ShortDescription = dto.ShortDescription;
            existing.NarrationText = dto.NarrationText;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{poiId}/{lang}")]
        public async Task<IActionResult> Delete(string poiId, string lang)
        {
            var translation = await _context.POITranslations
                .FirstOrDefaultAsync(t => t.POIID == poiId && t.LanguageCode == lang);

            if (translation == null) return NotFound();

            _context.POITranslations.Remove(translation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
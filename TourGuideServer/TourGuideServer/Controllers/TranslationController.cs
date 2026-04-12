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

        // GET: api/Translation/{poiId}
        [HttpGet("{poiId}")]
        public async Task<IActionResult> GetByPOI(int poiId)
        {
            var translations = await _context.POITranslations
                .Where(t => t.POIID == poiId)
                .OrderBy(t => t.LanguageCode)
                .ToListAsync();
            return Ok(translations);
        }

        // POST: api/Translation
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] POITranslation translation)
        {
            if (translation == null) return BadRequest();

            // Kiểm tra trùng (POIID, LanguageCode)
            var exists = await _context.POITranslations
                .AnyAsync(t => t.POIID == translation.POIID &&
                               t.LanguageCode == translation.LanguageCode);
            if (exists)
                return Conflict(new { message = $"Bản dịch '{translation.LanguageCode}' cho POI #{translation.POIID} đã tồn tại." });

            translation.TranslationID = 0;
            _context.POITranslations.Add(translation);
            await _context.SaveChangesAsync();
            return Ok(translation);
        }

        // PUT: api/Translation/{poiId}/{lang}
        [HttpPut("{poiId}/{lang}")]
        public async Task<IActionResult> Update(int poiId, string lang, [FromBody] POITranslation dto)
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

        // DELETE: api/Translation/{poiId}/{lang}
        [HttpDelete("{poiId}/{lang}")]
        public async Task<IActionResult> Delete(int poiId, string lang)
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
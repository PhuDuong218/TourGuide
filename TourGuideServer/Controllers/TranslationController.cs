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
        public async Task<ActionResult<IEnumerable<POITranslation>>> GetByPOI(int poiId)
        {
            var translations = await _context.POITranslations
                .Where(t => t.POIID == poiId)
                .ToListAsync();

            return Ok(translations);
        }

        // POST: api/Translation
        [HttpPost]
        public async Task<ActionResult<POITranslation>> Create([FromBody] POITranslation translation)
        {
            if (translation == null) return BadRequest();

            // Xóa Id để Database tự sinh (Identity), tránh xung đột
            translation.TranslationID = 0;

            _context.POITranslations.Add(translation);
            await _context.SaveChangesAsync();
            return Ok(translation);
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
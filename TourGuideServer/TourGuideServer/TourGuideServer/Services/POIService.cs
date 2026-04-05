using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.DTOs;
using TourGuideServer.Models;

namespace TourGuideServer.Services
{
    public class POIService
    {
        private readonly AppDbContext _context;

        public POIService(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 1. Lấy danh sách POI theo ngôn ngữ (có fallback)
        public async Task<List<POIDTO>> GetPOIsAsync(string lang)
        {
            // Nếu không có ngôn ngữ, mặc định lấy tiếng Việt (vi)
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            return await _context.POIs
                .Include(p => p.Translations)
                .Select(p => new POIDTO
                {
                    POIID = p.POIID,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,

                    // Ưu tiên ngôn ngữ yêu cầu, nếu không có thì lấy bất kỳ bản dịch nào sẵn có
                    Name = p.Translations.Where(t => t.LanguageCode == lang).Select(t => t.DisplayName).FirstOrDefault()
                           ?? p.Translations.Select(t => t.DisplayName).FirstOrDefault()
                           ?? "Chưa đặt tên",

                    Description = p.Translations.Where(t => t.LanguageCode == lang).Select(t => t.ShortDescription).FirstOrDefault()
                           ?? p.Translations.Select(t => t.ShortDescription).FirstOrDefault()
                           ?? "Không có mô tả",

                    Narration = p.Translations.Where(t => t.LanguageCode == lang).Select(t => t.NarrationText).FirstOrDefault()
                           ?? ""
                })
                .ToListAsync();
        }

        // 🔥 2. Lấy chi tiết 1 POI
        public async Task<POIDTO?> GetPOIByIdAsync(int id, string lang)
        {
            return await _context.POIs
                .Include(p => p.Translations)
                .Where(p => p.POIID == id)
                .Select(p => new POIDTO
                {
                    POIID = p.POIID,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,

                    Name = p.Translations
                        .Where(t => t.LanguageCode == lang)
                        .Select(t => t.DisplayName)
                        .FirstOrDefault()
                        ?? "No name",

                    Description = p.Translations
                        .Where(t => t.LanguageCode == lang)
                        .Select(t => t.ShortDescription)
                        .FirstOrDefault()
                        ?? "",

                    Narration = p.Translations
                        .Where(t => t.LanguageCode == lang)
                        .Select(t => t.NarrationText)
                        .FirstOrDefault()
                        ?? ""
                })
                .FirstOrDefaultAsync();
        }

        // 🔥 3. Lưu lịch sử truy cập
        public async Task SaveVisitAsync(int userId, int poiId)
        {
            var history = new VisitHistory
            {
                UserID = userId,
                POIID = poiId,
                VisitTime = DateTime.Now
            };

            _context.VisitHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        // 🔥 4. Tạo POI mới (basic)
        public async Task CreatePOIAsync(POI poi)
        {
            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();
        }

        // 🔥 5. Xóa POI
        public async Task<bool> DeletePOIAsync(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return false;

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
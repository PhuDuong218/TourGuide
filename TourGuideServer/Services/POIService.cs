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

        // ── Helper map POI → DTO ─────────────────────────────────────────────
        private static POIDTO MapToDTO(POI p, string lang)
        {
            var byLang = p.Translations.Where(t => t.LanguageCode == lang).ToList();
            var any = p.Translations.ToList();

            return new POIDTO
            {
                POIID = p.POIID,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Address = p.Address,
                Category = p.Category,
                ImageUrl = p.ImageUrl,   // ← thêm dòng này

                Name = byLang.Select(t => t.DisplayName).FirstOrDefault()
                    ?? any.Select(t => t.DisplayName).FirstOrDefault()
                    ?? "Chưa đặt tên",

                Description = byLang.Select(t => t.ShortDescription).FirstOrDefault()
                    ?? any.Select(t => t.ShortDescription).FirstOrDefault()
                    ?? "Không có mô tả",

                Narration = byLang.Select(t => t.NarrationText).FirstOrDefault()
                    ?? ""
            };
        }

        // ── 1. Lấy tất cả POI theo ngôn ngữ ─────────────────────────────────
        public async Task<List<POIDTO>> GetPOIsAsync(string lang)
        {
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            var pois = await _context.POIs
                .Include(p => p.Translations)
                .ToListAsync();

            return pois.Select(p => MapToDTO(p, lang)).ToList();
        }

        // ── 2. Lấy 1 POI theo ID ─────────────────────────────────────────────
        public async Task<POIDTO?> GetPOIByIdAsync(int id, string lang)
        {
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            var poi = await _context.POIs
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.POIID == id);

            return poi == null ? null : MapToDTO(poi, lang);
        }

        // ── 3. Lấy POI theo QR value ──────────────────────────────────────────
        // DB: bảng QRCode có QRValue → POIID
        public async Task<POIDTO?> GetPOIByQRAsync(string qrValue, string lang)
        {
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            var qr = await _context.QRCodes
                .Include(q => q.POI)
                    .ThenInclude(p => p!.Translations)
                .FirstOrDefaultAsync(q => q.QRValue == qrValue);

            if (qr?.POI == null) return null;

            return MapToDTO(qr.POI, lang);
        }

        // ── 4. Lưu lịch sử truy cập ──────────────────────────────────────────
        public async Task SaveVisitAsync(
            int? userId, int poiId,
            string scanMethod,
            decimal? userLat = null, decimal? userLon = null,
            string? languageUsed = null)
        {
            var history = new VisitHistory
            {
                UserID = userId,
                POIID = poiId,
                VisitTime = DateTime.Now,
                ScanMethod = scanMethod,
                UserLat = userLat,
                UserLon = userLon,
                LanguageUsed = languageUsed
            };

            _context.VisitHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        // ── 5. Tạo POI mới ────────────────────────────────────────────────────
        public async Task<POI> CreatePOIAsync(POI poi)
        {
            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();
            return poi;
        }

        // ── 6. Xóa POI (cascade tự xóa translations + QR) ────────────────────
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
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
                Img = p.Img,

                // Gán đúng vào RestaurantName (Ưu tiên tên dịch, nếu không có lấy tên gốc)
                RestaurantName = byLang.Select(t => t.DisplayName).FirstOrDefault()
                    ?? any.Select(t => t.DisplayName).FirstOrDefault()
                    ?? p.RestaurantName
                    ?? "Chưa đặt tên",

                ShortDescription = byLang.Select(t => t.ShortDescription).FirstOrDefault()
                    ?? any.Select(t => t.ShortDescription).FirstOrDefault()
                    ?? "Không có mô tả",

                NarrationText = byLang.Select(t => t.NarrationText).FirstOrDefault() ?? "",
                ViewCount = p.ViewCount,
                ListenCount = p.ListenCount,
                Priority = p.Priority
            };
        }

        // Đã thêm tham số ownerId vào đây để Controller có thể dùng
        public async Task<List<POIDTO>> GetPOIsAsync(string lang, string? ownerId = null)
        {
            var query = _context.POIs.Include(p => p.Translations).AsQueryable();

            if (!string.IsNullOrEmpty(ownerId))
            {
                query = query.Where(p => p.OwnerID == ownerId);
            }

            var pois = await query.OrderByDescending(p => p.POIID).ToListAsync();
            return pois.Select(p => MapToDTO(p, lang ?? "vi")).ToList();
        }

        public async Task<POIDTO?> GetPOIByIdAsync(string id, string lang)
        {
            var poi = await _context.POIs
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.POIID == id);
            return poi == null ? null : MapToDTO(poi, lang ?? "vi");
        }

        public async Task<POIDTO?> GetPOIByQRAsync(string qrValue, string lang)
        {
            var qr = await _context.QRCodes
                .Include(q => q.POI).ThenInclude(p => p!.Translations)
                .FirstOrDefaultAsync(q => q.QRValue == qrValue);
            return qr?.POI == null ? null : MapToDTO(qr.POI, lang ?? "vi");
        }

        public async Task SaveVisitAsync(string? userId, string poiId, string scanMethod, decimal? lat, decimal? lon, string? lang)
        {
            var history = new VisitHistory
            {
                VisitID = Guid.NewGuid().ToString().Substring(0, 10),
                UserID = userId,
                POIID = poiId,
                VisitTime = DateTime.Now,
                ScanMethod = scanMethod,
                UserLat = lat,
                UserLon = lon,
                LanguageUsed = lang
            };
            _context.VisitHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletePOIAsync(string id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return false;
            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
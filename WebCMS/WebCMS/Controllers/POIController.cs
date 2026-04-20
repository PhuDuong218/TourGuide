using GTranslate.Translators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebCMS.Models;
using WebCMS.Services;
using System.Security.Claims;

namespace WebCMS.Controllers
{
    [Authorize(Roles = "admin,owner")]
    public class POIController : Controller
    {
        private readonly IPOIService _poiService;
        private readonly TranslationService _translationService;
        private readonly IVisitHistoryService _visitHistoryService;

        public POIController(
            IPOIService poiService,
            TranslationService translationService,
            IVisitHistoryService visitHistoryService)
        {
            _poiService = poiService;
            _translationService = translationService;
            _visitHistoryService = visitHistoryService;
        }

        // ================= INDEX + DASHBOARD =================
        public async Task<IActionResult> Index()
        {
            var pois = await _poiService.GetAllAsync();
            var visitHistory = await _visitHistoryService.GetAllAsync();

            // ✅ FIX: dùng Claims thay vì Session
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner"))
            {
                pois = pois.Where(p => p.OwnerID == userId).ToList();

                visitHistory = visitHistory
                    .Where(v => pois.Any(p => p.Id == v.POIID))
                    .ToList();
            }

            ViewBag.ActiveUsers = visitHistory
                .Where(v => !string.IsNullOrEmpty(v.UserID))
                .Select(v => v.UserID)
                .Distinct()
                .Count();

            var topPoiId = visitHistory
                .Where(v => v.ScanMethod == "QR_Scan")
                .GroupBy(v => v.POIID)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            ViewBag.TopPOI = pois
                .FirstOrDefault(p => p.Id == topPoiId)
                ?.Name ?? "N/A";

            var poiDtos = pois.Select(p => new POIDTO
            {
                POIID = p.Id,                                   // ✅ Id → POIID
                Name = p.Name ?? "Chưa đặt tên",
                Description = p.Description ?? "",
                Latitude = (decimal)p.Latitude,
                Longitude = (decimal)p.Longitude,
                CategoryName = p.Category ?? "Chưa phân loại",
                Img = !string.IsNullOrEmpty(p.Image)            // ✅ Image → Img
                    ? $"https://gzm4vrwg-7054.asse.devtunnels.ms/uploads/{p.Image}"
                    : null
            }).ToList();

            return View(poiDtos);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(POI poi, IFormFile? imageFile)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                poi.OwnerID = userId;                           // ✅ string? = string?

                await _poiService.CreateAsync(poi, imageFile);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm địa điểm: " + ex.Message);
                return View(poi);
            }
        }

        // ================= EDIT =================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var poi = await _poiService.GetByIdAsync(id);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            return View(poi);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, POI poi, IFormFile? imageFile)
        {
            var existingPoi = await _poiService.GetByIdAsync(id);
            if (existingPoi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && existingPoi.OwnerID != userId)
                return Forbid();

            poi.Id = id;                                        // ✅ Id thay vì POIID

            try
            {
                await _poiService.UpdateAsync(id, poi, imageFile);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu: " + ex.Message);
                return View(poi);
            }
        }

        // ================= DELETE =================
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var poi = await _poiService.GetByIdAsync(id);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            await _poiService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // ================= TRANSLATION =================
        public async Task<IActionResult> Translation(string id)
        {
            var poi = await _poiService.GetByIdAsync(id);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            var list = await _translationService.GetByPOIAsync(id);
            ViewBag.POIID = id;
            return View(list);
        }

        public async Task<IActionResult> AddTranslation(string poiId)
        {
            var poi = await _poiService.GetByIdAsync(poiId);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            ViewBag.POIID = poiId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTranslation(POITranslation t)
        {
            var poi = await _poiService.GetByIdAsync(t.POIID);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            await _translationService.CreateAsync(t);
            return RedirectToAction("Translation", new { id = t.POIID });
        }

        public async Task<IActionResult> DeleteTranslation(string poiId, string lang)
        {
            var poi = await _poiService.GetByIdAsync(poiId);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            await _translationService.DeleteAsync(poiId, lang);
            return RedirectToAction("Translation", new { id = poiId });
        }

        // --- THÊM CHỨC NĂNG SỬA BẢN DỊCH ---

        [HttpGet]
        public async Task<IActionResult> EditTranslation(string poiId, string lang)
        {
            // Lấy toàn bộ bản dịch của POI này
            var listTrans = await _translationService.GetByPOIAsync(poiId);

            // Tìm ra bản dịch có ngôn ngữ (lang) tương ứng để đem đi sửa
            var translation = listTrans.FirstOrDefault(t => t.LanguageCode == lang);
            if (translation == null)
            {
                return NotFound();
            }

            return View(translation);
        }

        [HttpPost]
        public async Task<IActionResult> EditTranslation(POITranslation model)
        {
            if (ModelState.IsValid)
            {
                // Gọi Service để cập nhật xuống Database
                // (Lưu ý: Đảm bảo trong TranslationService của bạn đã có hàm UpdateAsync nhé)
                await _translationService.UpdateAsync(model);

                return RedirectToAction("Translation", new { id = model.POIID });
            }
            return View(model);
        }

    }
}
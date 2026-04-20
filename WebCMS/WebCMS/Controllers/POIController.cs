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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Phân quyền Owner chỉ thấy điểm của mình (Admin thì đi thẳng qua, thấy hết)
            if (User.IsInRole("owner"))
            {
                // Thêm .Trim() vào cả 2 vế để so sánh chính xác tuyệt đối
                pois = pois.Where(p => p.OwnerID != null && p.OwnerID.Trim() == userId.Trim()).ToList();
            }

            // 2. Tính Tổng số Bản Dịch của các điểm đang hiển thị
            int totalTranslations = 0;
            foreach (var poi in pois)
            {
                var translations = await _translationService.GetByPOIAsync(poi.POIID);
                if (translations != null)
                {
                    totalTranslations += translations.Count();
                }
            }
            ViewBag.TotalTranslations = totalTranslations;

            // 3. Map sang DTO để hiển thị giao diện
            var poiDtos = pois.Select(p => new POIDTO
            {
                POIID = p.POIID,
                RestaurantName = p.RestaurantName ?? "Chưa đặt tên",
                Latitude = (decimal)p.Latitude,
                Longitude = (decimal)p.Longitude,
                Category = p.Category ?? "Chưa phân loại",
                Img = !string.IsNullOrEmpty(p.Img)
                    ? $"https://gzm4vrwg-7054.asse.devtunnels.ms/uploads/{p.Img}"
                    : null,
                ViewCount = p.ViewCount,
                ListenCount = p.ListenCount,
                Priority = p.Priority
            }).ToList();

            return View(poiDtos);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create()
        {
            // 🔥 NẾU LÀ ADMIN: Gửi danh sách các Owner ra giao diện để Admin chọn
            if (User.IsInRole("admin"))
            {
                // Tạm thời hardcode danh sách Owner giống bên AccountController để demo
                // Sau này bạn có DB thì gọi từ DB lên nhé: await _userService.GetOwnersAsync()
                ViewBag.OwnerList = new List<dynamic>
                {
                    new { UserID = "U002", Username = "owner1" },
                    new { UserID = "U005", Username = "owner2" }
                };
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(POI poi, IFormFile? imageFile)
        {
            try
            {
                // 🔥 NẾU LÀ OWNER: Ép cứng ID của Owner đó (không quan tâm form gửi lên gì)
                if (User.IsInRole("owner"))
                {
                    poi.OwnerID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }
                // 🔥 NẾU LÀ ADMIN: poi.OwnerID đã tự động được lấy từ thẻ <select> trên giao diện (View) do Admin chọn!

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

            // Chặn Owner sửa POI của người khác (Admin thì được phép)
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

            // Chặn Owner lưu POI của người khác
            if (User.IsInRole("owner") && existingPoi.OwnerID != userId)
                return Forbid();

            poi.POIID = id;

            // Giữ nguyên OwnerID cũ nếu người sửa là Owner (tránh bị hack đổi OwnerID qua giao diện)
            if (User.IsInRole("owner"))
            {
                poi.OwnerID = userId;
            }

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

            // Chặn Owner xóa POI của người khác
            if (User.IsInRole("owner") && poi.OwnerID != userId)
                return Forbid();

            await _poiService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // ================= TRANSLATION =================
        // (Các hàm Translation tôi giữ nguyên vì logic chặn chặn quyền truy cập của bạn đã rất tốt rồi)

        public async Task<IActionResult> Translation(string id)
        {
            var poi = await _poiService.GetByIdAsync(id);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("owner") && poi.OwnerID != userId) return Forbid();

            var list = await _translationService.GetByPOIAsync(id);
            ViewBag.POIID = id;
            return View(list);
        }

        public async Task<IActionResult> AddTranslation(string poiId)
        {
            var poi = await _poiService.GetByIdAsync(poiId);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("owner") && poi.OwnerID != userId) return Forbid();

            ViewBag.POIID = poiId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTranslation(POITranslation t)
        {
            var poi = await _poiService.GetByIdAsync(t.POIID);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("owner") && poi.OwnerID != userId) return Forbid();

            await _translationService.CreateAsync(t);
            return RedirectToAction("Translation", new { id = t.POIID });
        }

        public async Task<IActionResult> DeleteTranslation(string poiId, string lang)
        {
            var poi = await _poiService.GetByIdAsync(poiId);
            if (poi == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole("owner") && poi.OwnerID != userId) return Forbid();

            await _translationService.DeleteAsync(poiId, lang);
            return RedirectToAction("Translation", new { id = poiId });
        }

        [HttpGet]
        public async Task<IActionResult> EditTranslation(string poiId, string lang)
        {
            var listTrans = await _translationService.GetByPOIAsync(poiId);
            var translation = listTrans.FirstOrDefault(t => t.LanguageCode == lang);
            if (translation == null) return NotFound();
            return View(translation);
        }

        [HttpPost]
        public async Task<IActionResult> EditTranslation(POITranslation model)
        {
            if (ModelState.IsValid)
            {
                await _translationService.UpdateAsync(model);
                return RedirectToAction("Translation", new { id = model.POIID });
            }
            return View(model);
        }
    }
}
using GTranslate.Translators;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Models;
using WebCMS.Services;

namespace WebCMS.Controllers
{
    public class POIController : Controller
    {
        private readonly IPOIService _poiService;
        private readonly TranslationService _translationService;

        public POIController(IPOIService poiService, TranslationService translationService)
        {
            _poiService = poiService;
            _translationService = translationService;
        }

        public async Task<IActionResult> Index()
        {
            var pois = await _poiService.GetAllAsync();
            var poiDtos = pois.Select(p => new POIDTO
            {
                POIID = p.POIID,

                // Gán tên địa điểm (nếu Model của bạn đổi thành RestaurantName thì dùng p.RestaurantName)
                RestaurantName = p.RestaurantName ?? "Chưa đặt tên",

                Address = p.Address ?? "",

                Latitude = (decimal)p.Latitude,
                Longitude = (decimal)p.Longitude,
                Category = p.Category ?? "Chưa phân loại",
                ViewCount = p.ViewCount,
                ListenCount = p.ListenCount,
                Priority = p.Priority,

                Img = !string.IsNullOrEmpty(p.Img)
                      ? $"https://gzm4vrwg-7054.asse.devtunnels.ms/uploads/{p.Img}"
                      : null
            }).ToList();

            return View(poiDtos);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(POI poi, IFormFile? imageFile)
        {
            try
            {
                await _poiService.CreateAsync(poi, imageFile);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm địa điểm: " + ex.Message);
                return View(poi);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var poi = await _poiService.GetByIdAsync(id);
            if (poi == null) return NotFound();
            return View(poi);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, POI poi, IFormFile? imageFile)
        {
            poi.POIID = id;

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

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _poiService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Translation(string id)
        {
            var list = await _translationService.GetByPOIAsync(id);
            ViewBag.POIID = id;
            return View(list);
        }

        public IActionResult AddTranslation(string poiId)
        {
            ViewBag.POIID = poiId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTranslation(POITranslation model)
        {
            if (ModelState.IsValid)
            {
                // 1. LẤY TẤT CẢ ID ĐỂ TÌM SỐ LỚN NHẤT (VD: T040)
                var allTrans = await _translationService.GetAllAsync();
                int maxId = 0;
                foreach (var t in allTrans)
                {
                    if (!string.IsNullOrEmpty(t.TranslationID) && t.TranslationID.StartsWith("T"))
                    {
                        string numberPart = t.TranslationID.Substring(1); // Cắt chữ T, lấy số
                        if (int.TryParse(numberPart, out int currentId))
                        {
                            if (currentId > maxId) maxId = currentId;
                        }
                    }
                }

                // Hàm cục bộ giúp tự động tăng số thứ tự lên 1
                string GenerateNextId()
                {
                    maxId++;
                    return "T" + maxId.ToString("D3"); // Sinh ra T041, T042...
                }

                // Gán ID mới sinh ra cho bản dịch gốc
                model.TranslationID = GenerateNextId();
                await _translationService.CreateAsync(model);

                // ==========================================================
                // 2. PHÉP THUẬT: KÍCH HOẠT TỰ ĐỘNG DỊCH NẾU LÀ TIẾNG VIỆT
                // ==========================================================
                if (model.LanguageCode == "vi")
                {
                    var translator = new GoogleTranslator();

                    async Task TranslateAndSaveAsync(string targetLang)
                    {
                        string translatedName = (await translator.TranslateAsync(model.DisplayName, targetLang, "vi")).Translation;

                        string translatedDesc = string.IsNullOrEmpty(model.ShortDescription)
                            ? ""
                            : (await translator.TranslateAsync(model.ShortDescription, targetLang, "vi")).Translation;

                        string translatedNarration = (await translator.TranslateAsync(model.NarrationText, targetLang, "vi")).Translation;

                        var newTrans = new POITranslation
                        {
                            TranslationID = GenerateNextId(), // Gọi hàm sinh mã tiếp theo (VD: T042)
                            POIID = model.POIID,
                            LanguageCode = targetLang,
                            DisplayName = translatedName,
                            ShortDescription = translatedDesc,
                            NarrationText = translatedNarration
                        };

                        await _translationService.CreateAsync(newTrans);
                    }

                    // Dịch và lưu từng ngôn ngữ còn lại
                    await TranslateAndSaveAsync("en");
                    await TranslateAndSaveAsync("fr");
                    await TranslateAndSaveAsync("ja");
                }

                return RedirectToAction("Translation", new { id = model.POIID });
            }

            return View("AddTranslation", model);
        }

        public async Task<IActionResult> DeleteTranslation(string poiId, string lang)
        {
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
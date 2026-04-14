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
                Name = p.Name ?? "Chưa đặt tên",

                Description = p.Description ?? "",

                Latitude = (decimal)p.Latitude,
                Longitude = (decimal)p.Longitude,
                CategoryName = p.Category ?? "Chưa phân loại",

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
        public async Task<IActionResult> AddTranslation(POITranslation t)
        {
            await _translationService.CreateAsync(t);
            return RedirectToAction("Translation", new { id = t.POIID });
        }

        public async Task<IActionResult> DeleteTranslation(string poiId, string lang)
        {
            await _translationService.DeleteAsync(poiId, lang);
            return RedirectToAction("Translation", new { id = poiId });
        }
    }
}
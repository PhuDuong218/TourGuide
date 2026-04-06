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
                POIID = p.Id,
                Name = p.Name ?? "N/A",
                Description = p.Description ?? "",
                Latitude = (decimal)p.Latitude,
                Longitude = (decimal)p.Longitude,
                CategoryName = p.Category ?? "Chưa phân loại"
            }).ToList();

            return View(poiDtos);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(POI poi)
        {
            await _poiService.CreateAsync(poi);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _poiService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Translations(string id)
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
            return RedirectToAction("Translations", new { id = t.POIID });
        }

        public async Task<IActionResult> DeleteTranslation(string poiId, string lang)
        {
            await _translationService.DeleteAsync(poiId, lang);
            return RedirectToAction("Translations", new { id = poiId });
        }
    }
}
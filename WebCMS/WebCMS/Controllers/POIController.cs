using Microsoft.AspNetCore.Mvc;
using WebCMS.Models;
using WebCMS.Services;

public class POIController : Controller
{
    private readonly IPOIService _poiService;
    private readonly TranslationService _translationService;

    public POIController(IPOIService poiService, TranslationService translationService)
    {
        _poiService = poiService;
        _translationService = translationService;
    }

    // 🔥 FIX async
   public async Task<IActionResult> Index()
{
    var pois = await _poiService.GetAllAsync();
    return View(pois);
}

    public IActionResult Create()
    {
        return View();
    }

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

    // 🔥 TRANSLATION
    public IActionResult Translations(int id)
    {
        var list = _translationService.GetByPOI(id);
        ViewBag.POIID = id;
        return View(list);
    }

    public IActionResult AddTranslation(int poiId)
    {
        ViewBag.POIID = poiId;
        return View();
    }

    [HttpPost]
    public IActionResult AddTranslation(POITranslation t)
    {
        _translationService.Create(t);
        return RedirectToAction("Translations", new { id = t.POIID });
    }

    public IActionResult DeleteTranslation(int poiId, string lang)
    {
        _translationService.Delete(poiId, lang);
        return RedirectToAction("Translations", new { id = poiId });
    }
}
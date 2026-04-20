using Microsoft.AspNetCore.Mvc;
using WebCMS.Services;

public class HomeController : Controller
{
    private readonly StatsService _statsService;
    public HomeController(StatsService statsService) => _statsService = statsService;

    public async Task<IActionResult> Index()
    {
        var model = await _statsService.GetFullDashboardAsync();
        return View(model);
    }

    // API cục bộ phục vụ việc cập nhật số người online mà không load lại trang
    [HttpGet]
    public async Task<JsonResult> GetRealTimeActiveUsers()
    {
        var model = await _statsService.GetSummaryAsync();
        return Json(new { count = model.ActiveToday });
    }
}
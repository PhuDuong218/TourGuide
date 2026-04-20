using Microsoft.AspNetCore.Mvc;
using WebCMS.Services;

namespace WebCMS.Controllers
{
    public class VisitHistoryController : Controller
    {
        private readonly VisitHistoryService _service;

        public VisitHistoryController(VisitHistoryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _service.GetAllAsync();
            // Sắp xếp lượt tham quan mới nhất lên đầu bàn
            var sortedData = data.OrderByDescending(x => x.VisitTime).ToList();
            return View(sortedData);
        }
    }
}
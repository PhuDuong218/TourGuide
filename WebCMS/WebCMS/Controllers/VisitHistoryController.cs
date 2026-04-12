using Microsoft.AspNetCore.Mvc;

namespace WebCMS.Controllers
{
    public class VisitHistoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace WebCMS.Controllers
{
    public class VisitHistoryController : Controller
    {
        private readonly IConfiguration _configuration;
        public VisitHistoryController(IConfiguration configuration) => _configuration = configuration;

        public IActionResult Index()
        {
            ViewBag.ApiUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }
    }
}